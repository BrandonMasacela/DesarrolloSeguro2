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

        public HomeController(ILogger<HomeController> logger,ResumenData resumenData, ClienteData clienteData, ResumenClienteData resumenClienteData, PrestamoData prestamoData)
        {
            _logger = logger;
            _resumenData = resumenData;
            _clienteData = clienteData;
            _resumenClienteData = resumenClienteData;
            _prestamoData = prestamoData;
        }
        /*public IActionResult Index(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                // Guardar el token en una cookie o contexto
                Response.Cookies.Append("access_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true
                });
            }
            return View();
        }*/
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
        public async Task<IActionResult> ObtenerResumen()
        {
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
            // Cerrar sesión en el esquema de cookies
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Eliminar cookies adicionales
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("MiCookieAuth"); // Nombre de tu cookie

            return RedirectToAction("Index", "Login");
        }
    }
}
