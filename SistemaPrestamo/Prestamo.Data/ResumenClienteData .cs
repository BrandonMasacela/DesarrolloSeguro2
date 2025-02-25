using Microsoft.Extensions.Options;
using Prestamo.Entidades;
using System.Data.SqlClient;
using System.Data;
namespace Prestamo.Data
{
    public class ResumenClienteData
    {
        private readonly ConnectionStrings con;
        public ResumenClienteData(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

        public async Task<ResumenCliente> ObtenerResumen(int idCliente, int idPrestamo)
        {
            ResumenCliente objeto = new ResumenCliente();

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerResumenPorCliente", conexion);
                cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                cmd.Parameters.AddWithValue("@IdPrestamo", idPrestamo);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        objeto = new ResumenCliente()
                        {
                            PrestamosCliente = dr["PrestamosPendientes"].ToString()!,
                            PagosClientePendientes = dr["PrestamosPagados"].ToString()!,
                        };
                    }
                }
            }
            return objeto;
        }

    }
}
