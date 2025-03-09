using Microsoft.Extensions.Options;
using Prestamo.Entidades;
using System.Data.SqlClient;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;


namespace Prestamo.Data
{
    public class PrestamoData
    {
        private readonly ConnectionStrings con;
        public PrestamoData(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

        public async Task<int> ObtenerIdPrestamoPorCliente(int idCliente)
        {
            int idPrestamo = 0;

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerIdPrestamoPorCliente", conexion);
                cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        idPrestamo = Convert.ToInt32(dr["IdPrestamo"]);
                    }
                }
            }
            return idPrestamo;
        }

        public async Task<string> Crear(Prestamo.Entidades.Prestamo objeto)
        {

            string respuesta = "";
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_crearPrestamo", conexion);
                cmd.Parameters.AddWithValue("@IdCliente", objeto.Cliente.IdCliente);
                cmd.Parameters.AddWithValue("@NroDocumento", objeto.Cliente.NroDocumento);
                cmd.Parameters.AddWithValue("@Nombre", objeto.Cliente.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", objeto.Cliente.Apellido);
                cmd.Parameters.AddWithValue("@Correo", objeto.Cliente.Correo);
                cmd.Parameters.AddWithValue("@Telefono", objeto.Cliente.Telefono);
                cmd.Parameters.AddWithValue("@IdMoneda", objeto.Moneda.IdMoneda);
                cmd.Parameters.AddWithValue("@FechaInicio", objeto.FechaInicioPago);
                cmd.Parameters.AddWithValue("@MontoPrestamo", objeto.MontoPrestamo);
                cmd.Parameters.AddWithValue("@InteresPorcentaje", objeto.InteresPorcentaje);
                cmd.Parameters.AddWithValue("@NroCuotas", objeto.NroCuotas);
                cmd.Parameters.AddWithValue("@FormaDePago", objeto.FormaDePago);
                cmd.Parameters.AddWithValue("@ValorPorCuota", objeto.ValorPorCuota);
                cmd.Parameters.AddWithValue("@ValorInteres", objeto.ValorInteres);
                cmd.Parameters.AddWithValue("@ValorTotal", objeto.ValorTotal);
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

        public async Task<List<Prestamo.Entidades.Prestamo>> ObtenerPrestamos(int Id, string NroDocumento)
        {
            List<Prestamo.Entidades.Prestamo> lista = new List<Prestamo.Entidades.Prestamo>();

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerPrestamos", conexion);
                cmd.Parameters.AddWithValue("@IdPrestamo", Id);
                cmd.Parameters.AddWithValue("@NroDocumento", NroDocumento);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteXmlReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        XDocument doc = XDocument.Load(dr);
                        lista = ((doc.Elements("Prestamos")) != null) ? (from prestamo in doc.Element("Prestamos")!.Elements("Prestamo")
                                                                             select new Prestamo.Entidades.Prestamo()
                                                                             {
                                                                                 IdPrestamo = Convert.ToInt32(prestamo.Element("IdPrestamo")!.Value),
                                                                                 Cliente = new Cliente()
                                                                                 {
                                                                                     IdCliente = Convert.ToInt32(prestamo.Element("IdCliente")!.Value),
                                                                                     NroDocumento = prestamo.Element("NroDocumento")!.Value,
                                                                                     Nombre = prestamo.Element("Nombre")!.Value,
                                                                                     Apellido = prestamo.Element("Apellido")!.Value,
                                                                                     Correo = prestamo.Element("Correo")!.Value,
                                                                                     Telefono = prestamo.Element("Telefono")!.Value
                                                                                 },
                                                                                 Moneda = new Moneda
                                                                                 {
                                                                                     IdMoneda = Convert.ToInt32(prestamo.Element("IdMoneda")!.Value),
                                                                                     Nombre = prestamo.Element("NombreMoneda")!.Value,
                                                                                     Simbolo = prestamo.Element("Simbolo")!.Value
                                                                                 },
                                                                                 FechaInicioPago = prestamo.Element("FechaInicioPago")!.Value,
                                                                                 MontoPrestamo = prestamo.Element("MontoPrestamo")!.Value,
                                                                                 InteresPorcentaje = prestamo.Element("InteresPorcentaje")!.Value,
                                                                                 NroCuotas = Convert.ToInt32(prestamo.Element("NroCuotas")!.Value),
                                                                                 FormaDePago = prestamo.Element("FormaDePago")!.Value,
                                                                                 ValorPorCuota = prestamo.Element("ValorPorCuota")!.Value,
                                                                                 ValorInteres = prestamo.Element("ValorInteres")!.Value,
                                                                                 ValorTotal = prestamo.Element("ValorTotal")!.Value,
                                                                                 Estado = prestamo.Element("Estado")!.Value,
                                                                                 FechaCreacion = prestamo.Element("FechaCreacion")!.Value,
                                                                                 PrestamoDetalle = prestamo.Elements("PrestamoDetalle") != null ? (from detalle in prestamo.Element("PrestamoDetalle")!.Elements("Detalle")
                                                                                                                                            select new PrestamoDetalle()
                                                                                                                                            {
                                                                                                                                                IdPrestamoDetalle = Convert.ToInt32(detalle.Element("IdPrestamoDetalle")!.Value),
                                                                                                                                                FechaPago = detalle.Element("FechaPago")!.Value,
                                                                                                                                                MontoCuota = detalle.Element("MontoCuota")!.Value,
                                                                                                                                                NroCuota = Convert.ToInt32(detalle.Element("NroCuota")!.Value),
                                                                                                                                                Estado = detalle.Element("Estado")!.Value,
                                                                                                                                                FechaPagado = detalle.Element("FechaPagado")!.Value
                                                                                                                                            }).ToList() : new List<PrestamoDetalle>()

                                                                             }).ToList() : new List<Prestamo.Entidades.Prestamo>();

                    }
                }
            }
            return lista;
        }


        public async Task<List<Prestamo.Entidades.Prestamo>> ObtenerPrestamos2(int Id, string NroDocumento)
        {
            List<Prestamo.Entidades.Prestamo> lista = new List<Prestamo.Entidades.Prestamo>();

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerPrestamos2", conexion);
                cmd.Parameters.AddWithValue("@IdPrestamo", Id);
                cmd.Parameters.AddWithValue("@NroDocumento", NroDocumento);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteXmlReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        XDocument doc = XDocument.Load(dr);
                        lista = ((doc.Elements("Prestamos")) != null) ? (from prestamo in doc.Element("Prestamos")!.Elements("Prestamo")
                                                                         select new Prestamo.Entidades.Prestamo()
                                                                         {
                                                                             IdPrestamo = Convert.ToInt32(prestamo.Element("IdPrestamo")!.Value),
                                                                             Cliente = new Cliente()
                                                                             {
                                                                                 IdCliente = Convert.ToInt32(prestamo.Element("IdCliente")!.Value),
                                                                                 NroDocumento = prestamo.Element("NroDocumento")!.Value,
                                                                                 Nombre = prestamo.Element("Nombre")!.Value,
                                                                                 Apellido = prestamo.Element("Apellido")!.Value,
                                                                                 Correo = prestamo.Element("Correo")!.Value,
                                                                                 Telefono = prestamo.Element("Telefono")!.Value
                                                                             },
                                                                             Moneda = new Moneda
                                                                             {
                                                                                 IdMoneda = Convert.ToInt32(prestamo.Element("IdMoneda")!.Value),
                                                                                 Nombre = prestamo.Element("NombreMoneda")!.Value,
                                                                                 Simbolo = prestamo.Element("Simbolo")!.Value
                                                                             },
                                                                             FechaInicioPago = prestamo.Element("FechaInicioPago")!.Value,
                                                                             MontoPrestamo = prestamo.Element("MontoPrestamo")!.Value,
                                                                             InteresPorcentaje = prestamo.Element("InteresPorcentaje")!.Value,
                                                                             NroCuotas = Convert.ToInt32(prestamo.Element("NroCuotas")!.Value),
                                                                             FormaDePago = prestamo.Element("FormaDePago")!.Value,
                                                                             ValorPorCuota = prestamo.Element("ValorPorCuota")!.Value,
                                                                             ValorInteres = prestamo.Element("ValorInteres")!.Value,
                                                                             ValorTotal = prestamo.Element("ValorTotal")!.Value,
                                                                             Estado = prestamo.Element("Estado")!.Value,
                                                                             FechaCreacion = prestamo.Element("FechaCreacion")!.Value,
                                                                             PrestamoDetalle = prestamo.Elements("PrestamoDetalle") != null ? (from detalle in prestamo.Element("PrestamoDetalle")!.Elements("Detalle")
                                                                                                                                               select new PrestamoDetalle()
                                                                                                                                               {
                                                                                                                                                   IdPrestamoDetalle = Convert.ToInt32(detalle.Element("IdPrestamoDetalle")!.Value),
                                                                                                                                                   FechaPago = detalle.Element("FechaPago")!.Value,
                                                                                                                                                   MontoCuota = detalle.Element("MontoCuota")!.Value,
                                                                                                                                                   NroCuota = Convert.ToInt32(detalle.Element("NroCuota")!.Value),
                                                                                                                                                   Estado = detalle.Element("Estado")!.Value,
                                                                                                                                                   FechaPagado = detalle.Element("FechaPagado")!.Value
                                                                                                                                               }).ToList() : new List<PrestamoDetalle>()

                                                                         }).ToList() : new List<Prestamo.Entidades.Prestamo>();

                    }
                }
            }
            return lista;
        }

        public async Task<string> PagarCuotas(int IdPrestamo, string NroCuotasPagadas, string NumeroTarjeta)
        {
            string respuesta = "";
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_pagarCuotas", conexion);
                cmd.Parameters.AddWithValue("@IdPrestamo", IdPrestamo);
                cmd.Parameters.AddWithValue("@NroCuotasPagadas", NroCuotasPagadas);
                cmd.Parameters.AddWithValue("@NumeroTarjeta", NumeroTarjeta);
                cmd.Parameters.Add("@msgError", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    respuesta = Convert.ToString(cmd.Parameters["@msgError"].Value)!;
                }
                catch (Exception ex)
                {
                    respuesta = "Error al procesar: " + ex.Message;
                }
            }
            return respuesta;
        }

        public async Task<bool> CrearSolicitudPrestamo(SolicitudPrestamo solicitud)
        {
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("sp_crearSolicitudPrestamo", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetros de entrada
                    cmd.Parameters.AddWithValue("@IdUsuario", solicitud.IdUsuario);
                    cmd.Parameters.AddWithValue("@Monto", solicitud.Monto);
                    cmd.Parameters.AddWithValue("@Plazo", solicitud.Plazo);
                    cmd.Parameters.AddWithValue("@Estado", solicitud.Estado);
                    cmd.Parameters.AddWithValue("@FechaSolicitud", solicitud.FechaSolicitud);
                    cmd.Parameters.AddWithValue("@Sueldo", solicitud.Sueldo);
                    cmd.Parameters.AddWithValue("@EsCasado", solicitud.EsCasado);
                    cmd.Parameters.AddWithValue("@NumeroHijos", solicitud.NumeroHijos);
                    cmd.Parameters.AddWithValue("@MetodoPago", solicitud.MetodoPago);
                    cmd.Parameters.AddWithValue("@Cedula", solicitud.Cedula);
                    cmd.Parameters.AddWithValue("@Ocupacion", solicitud.Ocupacion);

                    // Agregar parámetro de salida @msgError
                    SqlParameter outputParam = new SqlParameter("@msgError", SqlDbType.NVarChar, 200)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    // Ejecutar el procedimiento almacenado
                    await cmd.ExecuteNonQueryAsync();

                    // Capturar el mensaje de error
                    string mensajeError = outputParam.Value.ToString();

                    // Si hay un mensaje de error, lanzar una excepción
                    if (!string.IsNullOrEmpty(mensajeError))
                    {
                        throw new Exception(mensajeError);
                    }

                    return true;
                }
            }
        }

        public async Task<List<SolicitudPrestamo>> ObtenerSolicitudesPendientes()
        {
            List<SolicitudPrestamo> solicitudes = new List<SolicitudPrestamo>();

            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerSolicitudesPendientes", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        solicitudes.Add(new SolicitudPrestamo
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                            Monto = Convert.ToDecimal(dr["Monto"]),
                            Plazo = Convert.ToInt32(dr["Plazo"]),
                            Estado = dr["Estado"].ToString(),
                            FechaSolicitud = Convert.ToDateTime(dr["FechaSolicitud"]),
                            Sueldo = Convert.ToDecimal(dr["Sueldo"]),
                            EsCasado = Convert.ToBoolean(dr["EsCasado"]),
                            NumeroHijos = Convert.ToInt32(dr["NumeroHijos"]),
                            MetodoPago = dr["MetodoPago"].ToString(),
                            Cedula = dr["Cedula"].ToString(),
                            Ocupacion = dr["Ocupacion"].ToString()
                        });
                    }
                }
            }
            return solicitudes;
        }

        public async Task<bool> ActualizarEstadoSolicitud(int id, string estado)
        {

            try
            {
                using (var conexion = new SqlConnection(con.CadenaSQL))
                {
                    await conexion.OpenAsync();
                    Console.WriteLine(estado);
                    SqlCommand cmd = new SqlCommand("sp_actualizarEstadoSolicitud", conexion);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Estado", estado);
                    cmd.CommandType = CommandType.StoredProcedure;

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CrearHistorialCrediticio(Prestamo.Entidades.HistorialCrediticio historial)
        {
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_crearHistorialCrediticio", conexion);
                cmd.Parameters.AddWithValue("@IdUsuario", historial.IdUsuario);
                cmd.Parameters.AddWithValue("@EstadoCrediticio", historial.EstadoCrediticio);
                cmd.CommandType = CommandType.StoredProcedure;

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        public async Task<bool> ActualizarHistorialCrediticio(int idUsuario, bool aprobado)
        {
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_actualizarHistorialCrediticio", conexion);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@Aprobado", aprobado);
                cmd.CommandType = CommandType.StoredProcedure;

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        public async Task<HistorialCrediticio> ObtenerHistorialCrediticio(int idUsuario)
        {
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerHistorialCrediticio", conexion);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        return new HistorialCrediticio
                        {
                            IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                            EstadoCrediticio = Convert.ToInt32(dr["EstadoCrediticio"])
                        };
                    }
                }
            }
            return null;
        }

        public async Task<SolicitudPrestamo> ObtenerSolicitudPorId(int id)
        {
            using (var conexion = new SqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerSolicitudPorId", conexion);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        return new SolicitudPrestamo
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                            Monto = Convert.ToDecimal(dr["Monto"]),
                            Plazo = Convert.ToInt32(dr["Plazo"]),
                            Estado = dr["Estado"].ToString(),
                            FechaSolicitud = Convert.ToDateTime(dr["FechaSolicitud"]),
                            Sueldo = Convert.ToDecimal(dr["Sueldo"]),
                            EsCasado = Convert.ToBoolean(dr["EsCasado"]),
                            NumeroHijos = Convert.ToInt32(dr["NumeroHijos"]),
                            MetodoPago = dr["MetodoPago"].ToString(),
                            Cedula = dr["Cedula"].ToString(),
                            Ocupacion = dr["Ocupacion"].ToString()
                        };
                    }
                }
            }
            return null;
        }

    }
}
