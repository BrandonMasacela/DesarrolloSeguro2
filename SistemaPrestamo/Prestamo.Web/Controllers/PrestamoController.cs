using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prestamo.Data;
using Prestamo.Entidades;
using Prestamo.Web.Servives;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Prestamo.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PrestamoController : Controller
    {
        private readonly ClienteData _clienteData;
        private readonly PrestamoData _prestamoData;
        private readonly AuditoriaService _auditoriaService;
        public PrestamoController(ClienteData clienteData, PrestamoData prestamoData, AuditoriaService auditoriaService)
        {
            _clienteData = clienteData;
            _prestamoData = prestamoData;
            _auditoriaService = auditoriaService;
        }
        public IActionResult Index()
        {   
            return View();
        }

        public IActionResult Nuevo()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Cliente,Administrador")]
        public async Task<IActionResult> ObtenerCliente(string NroDocumento)
        {
            Cliente objeto = await _clienteData.Obtener(NroDocumento);
            return StatusCode(StatusCodes.Status200OK, new { data = objeto });
        }

        [HttpGet]
        [Authorize(Roles = "Cliente, Administrador")]
        public async Task<IActionResult> ObtenerCedulaCliente()
        {
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            Console.WriteLine(correo);
            if (string.IsNullOrEmpty(correo))
            {
                return RedirectToAction("Index", "Home");
            }

            var cliente = await _clienteData.ObtenerPorCorreo(correo);
            Console.WriteLine(cliente.NroDocumento);
            if (cliente == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return Ok(new { cedula = cliente.NroDocumento });

        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear([FromBody] Prestamo.Entidades.Prestamo objeto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string respuesta = await _prestamoData.Crear(objeto);
            await _auditoriaService.RegistrarLog(User.Identity.Name, "Crear", $"Préstamo creado: {objeto.IdPrestamo}");
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }


        [HttpGet]
        [Authorize(Roles = "Cliente, Administrador")]
        public async Task<IActionResult> ObtenerPrestamos(int IdPrestamo, string NroDocumento)
        {
            List<Prestamo.Entidades.Prestamo> objeto = await _prestamoData.ObtenerPrestamos(IdPrestamo,NroDocumento == null ? "": NroDocumento);
            return StatusCode(StatusCodes.Status200OK, new { data = objeto });
        }


        [HttpGet]
        [Authorize(Roles = "Cliente, Administrador")]
        public async Task<IActionResult> ObtenerPrestamos2(int IdPrestamo, string NroDocumento)
        {
            List<Prestamo.Entidades.Prestamo> objeto = await _prestamoData.ObtenerPrestamos2(IdPrestamo, NroDocumento == null ? "" : NroDocumento);
            return StatusCode(StatusCodes.Status200OK, new { data = objeto });
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ImprimirPrestamo(int IdPrestamo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            List<Prestamo.Entidades.Prestamo> lista = await _prestamoData.ObtenerPrestamos(IdPrestamo, "");
            Prestamo.Entidades.Prestamo objeto = lista[0];

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {

                    page.Margin(30);

                    page.Header().ShowOnce().Background("#D5D5D5").Padding(3).Row(row =>
                    {
                        row.RelativeItem().AlignLeft().Text("Préstamo").Bold().FontSize(14);
                        row.RelativeItem().AlignRight().Text($"Nro: {objeto.IdPrestamo}").Bold().FontSize(14);
                    });

                    page.Content().PaddingVertical(10).Column(col1 =>
                    {
                        col1.Spacing(18);

                        col1.Item().Column(col2 =>
                        {
                            col2.Spacing(5);
                            col2.Item().Row(row =>
                            {
                                row.RelativeItem().BorderBottom(1).AlignLeft().Text("Datos del Cliente").Bold().FontSize(12);
                            });
                            col2.Item().Row(row =>
                            {

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Numero Documento: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.Cliente.NroDocumento).FontSize(12);
                                    });
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Nombre: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.Cliente.Nombre).FontSize(12);
                                    });
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Apellido: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.Cliente.Apellido).FontSize(12);
                                    });
                                });
                                row.ConstantItem(50);
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Correo: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.Cliente.Correo).FontSize(12);
                                    });
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Telefono: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.Cliente.Telefono).FontSize(12);
                                    });
                                });
                            });
                        });

                        col1.Item().Column(col2 =>
                        {
                            col2.Spacing(5);
                            col2.Item().Row(row =>
                            {
                                row.RelativeItem().BorderBottom(1).AlignLeft().Text("Datos del Préstamo").Bold().FontSize(12);
                            });

                            col2.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Monto Prestado: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.MontoPrestamo).FontSize(12);
                                    });
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Interes %: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.InteresPorcentaje).FontSize(12);
                                    });
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Número Cuotas: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.NroCuotas.ToString()).FontSize(12);
                                    });
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Forma de pago: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.FormaDePago).FontSize(12);
                                    });
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Tipo Moneda: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.Moneda.Nombre).FontSize(12);
                                    });
                                });
                                row.ConstantItem(50);
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Monto por Cuota: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.ValorPorCuota).FontSize(12);
                                    });
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Monto Interes: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.ValorInteres).FontSize(12);
                                    });
                                    col.Item().Text(txt =>
                                    {
                                        txt.Span("Monto Total: ").SemiBold().FontSize(12);
                                        txt.Span(objeto.ValorTotal).FontSize(12);
                                    });
                                });
                            });
                        });


                        col1.Item().Column(col2 =>
                        {
                            col2.Spacing(5);
                            col2.Item().Row(row =>
                            {
                                row.RelativeItem().BorderBottom(1).AlignLeft().Text("Detalle Cuotas").Bold().FontSize(12);
                            });
                            col2.Item().Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();

                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Background("#D5D5D5")
                                    .Padding(4).Text("Nro. Cuota").FontColor("#000");

                                    header.Cell().Background("#D5D5D5")
                                   .Padding(4).Text("Fecha de Pago").FontColor("#000");

                                    header.Cell().Background("#D5D5D5")
                                   .Padding(4).Text("Monto a Pagar").FontColor("#000");

                                    header.Cell().Background("#D5D5D5")
                                   .Padding(4).Text("Estado").FontColor("#000");
                                });

                                foreach (var item in objeto.PrestamoDetalle)
                                {
                                  

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9")
                                        .Padding(4).Text(item.NroCuota.ToString()).FontSize(12);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9")
                                     .Padding(4).Text(item.FechaPago).FontSize(12);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9")
                                     .Padding(4).Text($"{objeto.Moneda.Simbolo} {item.MontoCuota}").FontSize(12);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9")
                                     .Padding(4).AlignRight().Text($"{item.Estado}").FontSize(12);
                                }

                            });
                        });

                    });


                    page.Footer()
                    .AlignRight()
                    .Text(txt =>
                    {
                        txt.Span("Pagina ").FontSize(10);
                        txt.CurrentPageNumber().FontSize(10);
                        txt.Span(" de ").FontSize(10);
                        txt.TotalPages().FontSize(10);
                    });
                });
            }).GeneratePdf();


            Stream pdfStream = new MemoryStream(pdf);
            return File(pdfStream, "application/pdf");
        }
    }
}
