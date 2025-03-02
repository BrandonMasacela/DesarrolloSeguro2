using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prestamo.Data;
using Prestamo.Web.Models;
using Prestamo.Web.Servives;

namespace Prestamo.Web.Controllers
{
    [ServiceFilter(typeof(ContentSecurityPolicyFilter))]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CobrarController : Controller
    {
        private readonly PrestamoData _prestamoData;
        private readonly AuditoriaService _auditoriaService;

        public CobrarController(PrestamoData prestamoData, AuditoriaService auditoriaService)
        {
            _prestamoData = prestamoData;
            _auditoriaService = auditoriaService;
        }
   
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> PagarCuotas([FromBody] PagarCuotasRequest request)
        {
            if (request == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.NumeroTarjeta))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "El número de tarjeta es requerido" });
            }

            if (request.IdPrestamo <= 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "El ID del préstamo es requerido" });
            }

            if (string.IsNullOrEmpty(request.NroCuotasPagadas))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Debe seleccionar al menos una cuota" });
            }

            try
            {
                string respuesta = await _prestamoData.PagarCuotas(
                    request.IdPrestamo,
                    request.NroCuotasPagadas,
                    request.NumeroTarjeta
                );

                if (respuesta.StartsWith("Error") || respuesta.Contains("incorrecto") || respuesta.Contains("insuficientes"))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { data = respuesta });
                }

                await _auditoriaService.RegistrarLog(User.Identity.Name, "Pagar", $"Cuota pagada: {request.NroCuotasPagadas}");
                return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { data = "Error al procesar el pago: " + ex.Message });
            }
        }

        public class PagarCuotasRequest
        {
            public int IdPrestamo { get; set; }
            public string NroCuotasPagadas { get; set; }
            public string NumeroTarjeta { get; set; }
        }
    }
}
