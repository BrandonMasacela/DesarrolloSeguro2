using System.Data;
using System.Data.SqlClient;
using Prestamo.Entidades;


namespace Prestamo.Data
{
    public class CuentaData
    {
        private readonly string _connectionString;

        public CuentaData(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Cuenta> ObtenerCuenta(int idCliente)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("sp_obtenerCuenta", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Cuenta
                            {
                                IdCuenta = reader.GetInt32(reader.GetOrdinal("IdCuenta")),
                                IdCliente = reader.GetInt32(reader.GetOrdinal("IdCliente")),
                                Tarjeta = reader.GetString(reader.GetOrdinal("Tarjeta")),
                                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                                Monto = reader.GetDecimal(reader.GetOrdinal("Monto"))
                            };
                        }
                        return null;
                    }
                }
            }
        }

        public async Task Depositar(int idCliente, decimal monto)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("sp_depositarCuenta", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                    cmd.Parameters.AddWithValue("@Monto", monto);

                    var msgError = new SqlParameter("@msgError", SqlDbType.VarChar, 100);
                    msgError.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(msgError);

                    await cmd.ExecuteNonQueryAsync();

                    if (!string.IsNullOrEmpty(msgError.Value?.ToString()))
                    {
                        throw new Exception(msgError.Value.ToString());
                    }
                }
            }
        }
    }
}