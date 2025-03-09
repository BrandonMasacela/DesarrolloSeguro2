using Microsoft.Extensions.Options;
using Prestamo.Entidades;
using System;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Prestamo.Data
{
    public class UsuarioData
    {
        private readonly ConnectionStrings con;
        public UsuarioData(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }
        public string GenerarClaveAleatoria(int longitud)
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var clave = new char[longitud];
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[longitud];
                rng.GetBytes(bytes);
                for (int i = 0; i < clave.Length; i++)
                {
                    clave[i] = caracteres[bytes[i] % caracteres.Length];
                }
            }
            Console.WriteLine(new string(clave));
            return new string(clave);
        }
        public async Task<Usuario> ObtenerPorCorreo(string correo)
        {
            Usuario objeto = null!;

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerUsuarioPorCorreo", conexion);
                cmd.Parameters.AddWithValue("@Correo", correo);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        objeto = new Usuario()
                        {
                            IdUsuario = Convert.ToInt32(dr["IdUsuario"].ToString()!),
                            NombreCompleto = dr["NombreCompleto"].ToString()!,
                            Correo = dr["Correo"].ToString()!,
                            Clave = dr["Clave"].ToString()!, // Asegúrate de incluir la contraseña hasheada
                            Rol = dr["Rol"].ToString()!,
                            FailedAttempts = Convert.ToInt32(dr["FailedAttempts"]),
                            IsLocked = Convert.ToBoolean(dr["IsLocked"]),
                            LockoutEnd = dr["LockoutEnd"] == DBNull.Value ? null : Convert.ToDateTime(dr["LockoutEnd"]),
                        };
                    }
                }
            }
            return objeto;
        }

        public async Task<bool> Crear(Usuario usuario)
        {
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_crearUsuario", conexion);
                cmd.Parameters.AddWithValue("@NombreCompleto", usuario.NombreCompleto);
                cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                cmd.Parameters.AddWithValue("@Clave", usuario.Clave); // Asegúrate de pasar este parámetro
                cmd.Parameters.AddWithValue("@Rol", usuario.Rol); // Nuevo parámetro
                cmd.CommandType = CommandType.StoredProcedure;

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        public async Task UpdateLockoutStatus(Usuario usuario)
        {
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_UpdateLockoutStatus", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
                cmd.Parameters.AddWithValue("@FailedAttempts", usuario.FailedAttempts);
                cmd.Parameters.AddWithValue("@LastFailedAttempt", usuario.LastFailedAttempt);
                cmd.Parameters.AddWithValue("@LockoutEnd", usuario.LockoutEnd ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsLocked", usuario.IsLocked);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task ResetLockout(int idUsuario)
        {
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_ResetLockout", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> Actualizar(Usuario usuario)
        {
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("sp_actualizarUsuario", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
                    cmd.Parameters.AddWithValue("@Clave", usuario.Clave);

                    // Agregar parámetro de salida para capturar el mensaje de error
                    SqlParameter outputParam = new SqlParameter("@msgError", SqlDbType.NVarChar, 200)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    await cmd.ExecuteNonQueryAsync();

                    // Capturar el mensaje de error después de la ejecución
                    string mensajeError = outputParam.Value.ToString();

                    if (!string.IsNullOrEmpty(mensajeError))
                    {
                        throw new Exception(mensajeError); // Lanza una excepción con el mensaje de error
                    }

                    return true;
                }
            }
        }

        public async Task<Usuario> ObtenerPorId(int idUsuario)
        {
            Usuario objeto = null!;
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerUsuarioPorId", conexion);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.CommandType = CommandType.StoredProcedure;
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        objeto = new Usuario()
                        {
                            IdUsuario = Convert.ToInt32(dr["IdUsuario"].ToString()!),
                            NombreCompleto = dr["NombreCompleto"].ToString()!,
                            Correo = dr["Correo"].ToString()!,
                            Clave = dr["Clave"].ToString()!, // Asegúrate de incluir la contraseña hasheada
                            Rol = dr["Rol"].ToString()!,
                            FailedAttempts = Convert.ToInt32(dr["FailedAttempts"]),
                            IsLocked = Convert.ToBoolean(dr["IsLocked"]),
                            LockoutEnd = dr["LockoutEnd"] == DBNull.Value ? null : Convert.ToDateTime(dr["LockoutEnd"]),
                        };
                    }
                }
            }
            Console.WriteLine(objeto);
            return objeto;
        }

    }
}