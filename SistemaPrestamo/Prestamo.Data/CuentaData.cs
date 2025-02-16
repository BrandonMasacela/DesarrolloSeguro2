using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using Prestamo.Entidades;


namespace Prestamo.Data
{
    public class CuentaData
    {
        private readonly ConnectionStrings con;

        public CuentaData(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

        public async Task<Cuenta> ObtenerCuenta(int idCliente)
        {
            Cuenta cuenta = null;

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerCuenta", conexion);
                cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        //Console.WriteLine($"Valor de FechaCreacion en BD: {dr["FechaCreacion"]}");
                        cuenta = new Cuenta()
                        {
                            IdCuenta = Convert.ToInt32(dr["IdCuenta"]),
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            Tarjeta = dr["Tarjeta"].ToString()!,
                            // Manejo más seguro de la fecha
                            //FechaCreacion = dr["FechaCreacion"] != DBNull.Value
                            //    ? Convert.ToDateTime(dr["FechaCreacion"])
                            //    : DateTime.Now,
                            Monto = Convert.ToDecimal(dr["Monto"])
                        };
                    }
                }
            }
            return cuenta;
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