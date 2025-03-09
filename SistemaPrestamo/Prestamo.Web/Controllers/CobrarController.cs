using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prestamo.Data;
using Prestamo.Web.Models;
using Prestamo.Web.Servives;
using System.Security.Claims;

namespace Prestamo.Web.Controllers
{
    [Authorize]
    public class CobrarController : Controller
    {
        private readonly PrestamoData _prestamoData;
        private readonly AuditoriaService _auditoriaService;
        private readonly ClienteData _clienteData;

        public CobrarController(PrestamoData prestamoData, AuditoriaService auditoriaService, ClienteData clienteData)
        {
            _prestamoData = prestamoData;
            _auditoriaService = auditoriaService;
            _clienteData = clienteData;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var correo = User.FindFirst(ClaimTypes.Email)?.Value;
                Console.WriteLine($"Correo encontrado: {correo}");

                if (string.IsNullOrEmpty(correo))
                {
                    Console.WriteLine("Correo no encontrado en los claims");
                    return RedirectToAction("Login", "Account");
                }

                var cliente = await _clienteData.ObtenerPorCorreo(correo);

                if (cliente == null)
                {
                    Console.WriteLine($"No se encontró cliente para el correo: {correo}");
                    return RedirectToAction("Login", "Account");
                }

                Console.WriteLine($"Cliente encontrado con ID: {cliente.IdCliente}");
                ViewBag.IdCliente = cliente.IdCliente;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Login", "Account");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> PagarCuotas([FromBody] PagarCuotasRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
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

                await _auditoriaService.RegistrarLog(User!.Identity!.Name!, "Pagar", $"Cuota pagada: {request.NroCuotasPagadas}");
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
