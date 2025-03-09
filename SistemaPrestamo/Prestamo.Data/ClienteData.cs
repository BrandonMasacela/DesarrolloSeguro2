using Microsoft.Extensions.Options;
using Prestamo.Entidades;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Prestamo.Data
{
    public class ClienteData
    {
        private readonly ConnectionStrings con;
        private readonly UsuarioData usuarioData;
        private readonly EmailService emailService;
        private readonly string encryptionKey = "aB3dE5fG7hI9jK1mN2oP4qR6sT8uV0wX";

        public ClienteData(IOptions<ConnectionStrings> options, UsuarioData usuarioData, EmailService emailService)
        {
            con = options.Value;
            this.usuarioData = usuarioData;
            this.emailService = emailService;
        }

        private string Encrypt(string plainText)
        {
            using (var aes = Aes.Create())
            {
                byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
                Array.Resize(ref key, 32); // 256 bits
                aes.Key = key;

                // Generar IV automáticamente (16 bytes)
                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    // Escribir IV seguido del texto cifrado
                    ms.Write(iv, 0, iv.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
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

        private string GenerarTarjeta()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[8]; // 8 bytes = 64 bits
                rng.GetBytes(bytes);
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    // Convertir cada byte a un número de dos dígitos
                    builder.Append((b % 10).ToString());
                    builder.Append(((b / 10) % 10).ToString());
                }
                // Asegurarse de que la longitud sea exactamente 16 dígitos
                return builder.ToString().Substring(0, 16);
            }
        }
        public async Task<string> CrearCuenta(Cuenta cuenta)
        {
            string respuesta = "";
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_crearCuenta", conexion);
                cmd.Parameters.AddWithValue("@IdCliente", cuenta.IdCliente);
                cmd.Parameters.Add("@Tarjeta", SqlDbType.VarChar, 256).Value = Encrypt(cuenta.Tarjeta);
                Console.WriteLine("Tarjeta encriptada: " + Encrypt(cuenta.Tarjeta));
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
                cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@msgError", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    respuesta = Convert.ToString(cmd.Parameters["@msgError"].Value)!;

                    if (respuesta == "")
                    {
                        // Obtener el IdCliente generado
                        objeto.IdCliente = Convert.ToInt32(cmd.Parameters["@IdCliente"].Value);

                        // Generar clave aleatoria
                        string claveAleatoria = usuarioData.GenerarClaveAleatoria(12);

                        // Reemplazar la línea problemática con:
                        string hashedClave = BCrypt.Net.BCrypt.HashPassword(claveAleatoria);

                        // Crear usuario
                        Usuario nuevoUsuario = new Usuario
                        {
                            NombreCompleto = objeto.Nombre + " " + objeto.Apellido,
                            Correo = objeto.Correo,
                            Clave = hashedClave,
                            Rol = "Cliente" // Asignar el rol adecuado
                        };

                        await usuarioData.Crear(nuevoUsuario);

                        // Enviar correo con la contraseña
                        string asunto = "Bienvenido a nuestro sistema";
                        string mensaje = $"Hola {objeto.Nombre + " " + objeto.Apellido},<br/><br/>Tu cuenta ha sido creada exitosamente. Tu contraseña es: <b>{claveAleatoria}</b><br/><br/>Saludos,<br/>El equipo de Prestamo";
                        await emailService.EnviarCorreoAsync(objeto.Correo, asunto, mensaje);

                        Cuenta nuevaCuenta = new Cuenta
                        {
                            IdCliente = objeto.IdCliente,
                            Tarjeta = GenerarTarjeta(),
                            Monto = 0
                        };

                        string respuestaCuenta = await CrearCuenta(nuevaCuenta);
                        if (!string.IsNullOrEmpty(respuestaCuenta))
                        {
                            throw new Exception(respuestaCuenta);
                        }
                    }
                }
                catch (Exception ex)
                {
                    respuesta = "Error al procesar: " + ex.Message;
                    Console.WriteLine("Error: " + ex.Message);
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

            // Validación del parámetro
            if (string.IsNullOrEmpty(correo))
            {
                throw new ArgumentException("El correo no puede estar vacío");
            }

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("sp_obtenerClientePorCorreo", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Asegurarse de que el parámetro no sea null
                    cmd.Parameters.AddWithValue("@Correo", correo ?? string.Empty);

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            cliente = new Cliente()
                            {
                                IdCliente = Convert.ToInt32(dr["IdCliente"]),
                                NroDocumento = dr["NroDocumento"].ToString(),
                                Nombre = dr["Nombre"].ToString(),
                                Apellido = dr["Apellido"].ToString(),
                                Correo = dr["Correo"].ToString(),
                                Telefono = dr["Telefono"].ToString(),
                                FechaCreacion = dr["FechaCreacion"].ToString()
                            };
                        }
                    }
                }
            }
            return cliente;
        }

        public async Task<Cliente> ObtenerPorCedula(string cedula)
        {
            Cliente cliente = null;

            // Validación del parámetro
            if (string.IsNullOrEmpty(cedula))
            {
                throw new ArgumentException("El cedula no puede estar vacío");
            }

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("sp_obtenerCliente", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Asegurarse de que el parámetro no sea null
                    cmd.Parameters.AddWithValue("@NroDocumento", cedula ?? string.Empty);

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            cliente = new Cliente()
                            {
                                IdCliente = Convert.ToInt32(dr["IdCliente"]),
                                NroDocumento = dr["NroDocumento"].ToString(),
                                Nombre = dr["Nombre"].ToString(),
                                Apellido = dr["Apellido"].ToString(),
                                Correo = dr["Correo"].ToString(),
                                Telefono = dr["Telefono"].ToString(),
                                FechaCreacion = dr["FechaCreacion"].ToString()
                            };
                        }
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
                SqlCommand cmd = new SqlCommand("sp_depositarCuenta", conexion);
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
