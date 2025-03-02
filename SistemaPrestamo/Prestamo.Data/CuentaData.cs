using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Prestamo.Entidades;


namespace Prestamo.Data
{
    public class CuentaData
    {
        private readonly ConnectionStrings con;
        private readonly string encryptionKey = "aB3dE5fG7hI9jK1mN2oP4qR6sT8uV0wX";

        public CuentaData(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }
        private string Decrypt(string cipherText)
        {
            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                // Asegurar que el array tiene al menos el tamaño del IV
                if (fullCipher.Length < 16)
                {
                    throw new ArgumentException("Texto cifrado inválido");
                }

                using (var aes = Aes.Create())
                {
                    // Extraer el IV (primeros 16 bytes)
                    byte[] iv = new byte[16];
                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);

                    // Extraer el texto cifrado (bytes restantes)
                    byte[] cipher = new byte[fullCipher.Length - iv.Length];
                    Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                    // Configurar la clave y el IV
                    byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
                    Array.Resize(ref key, 32); // Asegurar 256 bits (32 bytes)

                    aes.Key = key;
                    aes.IV = iv;

                    // Desencriptar
                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream(cipher))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar errores (log, etc.)
                return string.Empty;
            }
        }
        public async Task<Cuenta> ObtenerCuenta(int idCliente)
        {
            Cuenta cuenta = new Cuenta();

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerCuenta", conexion);
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
                            Tarjeta = Decrypt(dr["Tarjeta"].ToString()!),
                            // Manejo más seguro de la fecha
                            //FechaCreacion = dr["FechaCreacion"] != DBNull.Value
                            //    ? Convert.ToDateTime(dr["FechaCreacion"])
                            //    : DateTime.Now,
                            Monto = Convert.ToDecimal(dr["Monto"])
                        }; ;
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