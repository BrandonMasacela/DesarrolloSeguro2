using Microsoft.Extensions.Options;
using Prestamo.Entidades;
using System;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace Prestamo.Data
{
    public class UsuarioData
    {
        private readonly ConnectionStrings con;
        public UsuarioData(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
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
                            Rol = dr["Rol"].ToString()!
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
    }
}