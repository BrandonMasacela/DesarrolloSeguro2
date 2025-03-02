using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Prestamo.Data;
using Prestamo.Web.Servives;
using System.Security.Claims;

namespace Prestamo.Web.Controllers
{
    [ServiceFilter(typeof(ContentSecurityPolicyFilter))]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CuentaController : Controller
    {
        private readonly ClienteData _clienteData;
        private readonly CuentaData _cuentaData;

        public CuentaController(ClienteData clienteData, CuentaData cuentaData)
        {
            _clienteData = clienteData;
            _cuentaData = cuentaData;
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

        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> ObtenerCuenta(int idCliente)
        {
            Console.WriteLine($"Obteniendo cuenta para cliente ID: {idCliente}");
           
            try
            {
                var cuenta = await _cuentaData.ObtenerCuenta(idCliente);
                if (cuenta == null)
                {
                    return Json(new { success = false, message = "No se encontró la cuenta" });
                }

                Console.WriteLine($"Cuenta encontrada: {cuenta?.Tarjeta ?? "null"}");
                return Json(new { success = true, data = cuenta });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener cuenta: {ex.Message}");
                return Json(new { success = false, message = "Error al obtener la cuenta" });
            }
        }

    }
}
