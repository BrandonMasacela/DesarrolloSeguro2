using Microsoft.Extensions.Options;
using Prestamo.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prestamo.Data
{
    public class AuditoriaData
    {
        private readonly ConnectionStrings _connectionStrings;

        public AuditoriaData(IOptions<ConnectionStrings> options)
        {
            _connectionStrings = options.Value;
        }

        public async Task<string> Insertar(Auditoria log)
        {
            string respuesta = "";
            using (var conexion = new SqlConnection(_connectionStrings.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_insertarAuditoria", conexion);
                cmd.Parameters.AddWithValue("@Usuario", log.Usuario);
                cmd.Parameters.AddWithValue("@Accion", log.Accion);
                cmd.Parameters.AddWithValue("@Fecha", log.Fecha);
                cmd.Parameters.AddWithValue("@Detalles", log.Detalles);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    respuesta = "Log registrado correctamente";
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