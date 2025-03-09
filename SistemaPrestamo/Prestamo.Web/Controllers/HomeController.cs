using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prestamo.Data;
using Prestamo.Entidades;
using Prestamo.Web.Models;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Prestamo.Web.Servives;

namespace Prestamo.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ResumenData _resumenData;
        private readonly ClienteData _clienteData;
        private readonly ResumenClienteData _resumenClienteData;
        private readonly PrestamoData _prestamoData;
        private readonly AuditoriaService _auditoriaService;

        public HomeController(ILogger<HomeController> logger,ResumenData resumenData, ClienteData clienteData, ResumenClienteData resumenClienteData, PrestamoData prestamoData, AuditoriaService auditoriaService)
        {
            _logger = logger;
            _resumenData = resumenData;
            _clienteData = clienteData;
            _resumenClienteData = resumenClienteData;
            _prestamoData = prestamoData;
            _auditoriaService = auditoriaService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObtenerResumen()
        {

            var usuario = User.Identity!.Name;
            await _auditoriaService.RegistrarLog(usuario!, "ObtenerResumen", "El usuario solicit� el resumen administrativo.");

            Resumen objeto = await _resumenData.Obtener();
            return StatusCode(StatusCodes.Status200OK, new { data = objeto });
        }

        [HttpGet]
        public IActionResult ObtenerRolUsuario()
        {
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            return Ok(new { roles });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerResumenCliente(int idPrestamo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var usuario = User.Identity!.Name;
            await _auditoriaService.RegistrarLog(usuario!, "ObtenerResumenCliente", "El usuario solicit� el resumen del cliente.");

            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            Console.WriteLine(correo);
            if (string.IsNullOrEmpty(correo))
            {
                return RedirectToAction("Index", "Home");
            }

            var cliente = await _clienteData.ObtenerPorCorreo(correo);
            var prestamo = await _prestamoData.ObtenerIdPrestamoPorCliente(cliente.IdCliente);
            Console.WriteLine(prestamo);
            if (cliente != null)
            {
                var resumen = await _resumenClienteData.ObtenerResumen(cliente.IdCliente, prestamo);
                Console.WriteLine(resumen.PagosClientePendientes);
                Console.WriteLine(resumen.PrestamosCliente);
                return Ok(resumen);
            }
            return NotFound();
        }

        public async Task<IActionResult> Salir()
        {
            var usuario = User.Identity!.Name;
            await _auditoriaService.RegistrarLog(usuario!, "Salir", "El usuario cerr� sesi�n.");
            // Cerrar sesi�n en el esquema de cookies
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Eliminar cookies adicionales
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("MiCookieAuth"); // Nombre de tu cookie

            return RedirectToAction("Index", "Login");
        }
    }
}
