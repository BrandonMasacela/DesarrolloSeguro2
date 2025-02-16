using Microsoft.Extensions.Options;
using Prestamo.Entidades;
using System.Data.SqlClient;
using System.Data;

namespace Prestamo.Data
{
    public class ClienteData
    {
        private readonly ConnectionStrings con;
        private readonly UsuarioData usuarioData;
        private readonly EmailService emailService;

        public ClienteData(IOptions<ConnectionStrings> options, UsuarioData usuarioData, EmailService emailService)
        {
            con = options.Value;
            this.usuarioData = usuarioData;
            this.emailService = emailService;
        }

        public async Task<List<Cliente>> Lista()
        {
            List<Cliente> lista = new List<Cliente>();

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_listaCliente", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new Cliente()
                        {
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            NroDocumento = dr["NroDocumento"].ToString()!,
                            Nombre = dr["Nombre"].ToString()!,
                            Apellido = dr["Apellido"].ToString()!,
                            Correo = dr["Correo"].ToString()!,
                            Telefono = dr["Telefono"].ToString()!,
                            FechaCreacion = dr["FechaCreacion"].ToString()!
                        });
                    }
                }
            }
            return lista;
        }
        public async Task<Cliente> Obtener(string NroDocumento)
        {
            Cliente objeto = new Cliente();

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerCliente", conexion);
                cmd.Parameters.AddWithValue("@NroDocumento", NroDocumento);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        objeto = new Cliente()
                        {
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            NroDocumento = dr["NroDocumento"].ToString()!,
                            Nombre = dr["Nombre"].ToString()!,
                            Apellido = dr["Apellido"].ToString()!,
                            Correo = dr["Correo"].ToString()!,
                            Telefono = dr["Telefono"].ToString()!,
                            FechaCreacion = dr["FechaCreacion"].ToString()!
                        };
                    }
                }
            }
            return objeto;
        }
        public async Task<string> CrearCuenta(Cuenta cuenta)
        {
            string respuesta = "";
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_crearCuenta", conexion);
                cmd.Parameters.AddWithValue("@IdCliente", cuenta.IdCliente);
                cmd.Parameters.AddWithValue("@Tarjeta", cuenta.Tarjeta);
                cmd.Parameters.AddWithValue("@Monto", cuenta.Monto);
                cmd.Parameters.Add("@msgError", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    respuesta = Convert.ToString(cmd.Parameters["@msgError"].Value)!;
                }
                catch
                {
                    respuesta = "Error al procesar";
                }
            }
            return respuesta;
        }

        public async Task<Cuenta> ObtenerCuentaPorCliente(int idCliente)
        {
            Cuenta cuenta = new Cuenta();

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerCuentaPorCliente", conexion);
                cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        cuenta = new Cuenta()
                        {
                            IdCuenta = Convert.ToInt32(dr["IdCuenta"]),
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            Tarjeta = dr["Tarjeta"].ToString()!,
                            FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                            Monto = Convert.ToDecimal(dr["Monto"])
                        };
                    }
                }
            }
            return cuenta;
        }
        public async Task<string> Crear(Cliente objeto)
        {
            string respuesta = "";
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_crearCliente", conexion);
                cmd.Parameters.AddWithValue("@NroDocumento", objeto.NroDocumento);
                cmd.Parameters.AddWithValue("@Nombre", objeto.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", objeto.Apellido);
                cmd.Parameters.AddWithValue("@Correo", objeto.Correo);
                cmd.Parameters.AddWithValue("@Telefono", objeto.Telefono);
                cmd.Parameters.Add("@msgError", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    respuesta = Convert.ToString(cmd.Parameters["@msgError"].Value)!;

                    if (respuesta == "")
                    {
                        // Generar clave aleatoria
                        string claveAleatoria = usuarioData.GenerarClaveAleatoria(12);

                        // Reemplazar la línea problemática con:
                        string hashedClave = BCrypt.Net.BCrypt.HashPassword(claveAleatoria);

                        // Crear usuario
                        Usuario nuevoUsuario = new Usuario
                        {
                            NombreCompleto = objeto.Nombre +" "+ objeto.Apellido,
                            Correo = objeto.Correo,
                            Clave = hashedClave,
                            Rol = "Cliente" // Asignar el rol adecuado
                        };

                        await usuarioData.Crear(nuevoUsuario);

                        // Enviar correo con la contraseña
                        string asunto = "Bienvenido a nuestro sistema";
                        string mensaje = $"Hola {objeto.Nombre +" "+ objeto.Apellido},<br/><br/>Tu cuenta ha sido creada exitosamente. Tu contraseña es: <b>{claveAleatoria}</b><br/><br/>Saludos,<br/>El equipo de Prestamo";
                        await emailService.EnviarCorreoAsync(objeto.Correo, asunto, mensaje);
                    }
                }
                catch
                {
                    respuesta = "Error al procesar";
                }
            }
            return respuesta;
        }

        public async Task<string> Editar(Cliente objeto)
        {

            string respuesta = "";
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_editarCliente", conexion);
                cmd.Parameters.AddWithValue("@IdCliente", objeto.IdCliente);
                cmd.Parameters.AddWithValue("@NroDocumento", objeto.NroDocumento);
                cmd.Parameters.AddWithValue("@Nombre", objeto.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", objeto.Apellido);
                cmd.Parameters.AddWithValue("@Correo", objeto.Correo);
                cmd.Parameters.AddWithValue("@Telefono", objeto.Telefono);
                cmd.Parameters.Add("@msgError", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    respuesta = Convert.ToString(cmd.Parameters["@msgError"].Value)!;
                }
                catch
                {
                    respuesta = "Error al procesar";
                }
            }
            return respuesta;
        }

        public async Task<string> Eliminar(int Id)
        {

            string respuesta = "";
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_eliminarCliente", conexion);
                cmd.Parameters.AddWithValue("@IdCliente", Id);
                cmd.Parameters.Add("@msgError", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    respuesta = Convert.ToString(cmd.Parameters["@msgError"].Value)!;
                }
                catch
                {
                    respuesta = "Error al procesar";
                }
            }
            return respuesta;
        }

        public async Task<Cliente> ObtenerPorCorreo(string correo)
        {
            Cliente cliente = null;
            Console.WriteLine(correo);

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerClientePorCorreo", conexion);
                cmd.Parameters.AddWithValue("@Correo", correo);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        cliente = new Cliente()
                        {
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            NroDocumento = dr["NroDocumento"].ToString()!,
                            Nombre = dr["Nombre"].ToString()!,
                            Apellido = dr["Apellido"].ToString()!,
                            Correo = dr["Correo"].ToString()!,
                            Telefono = dr["Telefono"].ToString()!,
                            FechaCreacion = dr["FechaCreacion"].ToString()!
                        };
                    }
                }
            }
            return cliente;
        }

        public async Task<string> Depositar(int idCliente, decimal monto)
        {
            string respuesta = "";
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_depositar", conexion);
                cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                cmd.Parameters.AddWithValue("@Monto", monto);
                cmd.Parameters.Add("@msgError", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    respuesta = Convert.ToString(cmd.Parameters["@msgError"].Value)!;
                }
                catch
                {
                    respuesta = "Error al procesar";
                }
            }
            return respuesta;
        }
    }
}
