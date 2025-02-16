using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prestamo.Data;
using Prestamo.Web.Models;

namespace Prestamo.Web.Controllers
{
    [Authorize]
    public class CobrarController : Controller
    {
        private readonly ClienteData _clienteData;
        private readonly PrestamoData _prestamoData;

        public CobrarController(ClienteData clienteData, PrestamoData prestamoData)
        {
            _clienteData = clienteData;
            _prestamoData = prestamoData;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
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
