using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prestamo.Data;
using Prestamo.Entidades;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;


namespace Prestamo.Web.Controllers
{
    [Authorize]
    public class LoginController : Controller
    {
        private readonly UsuarioData _usuarioData;

        public LoginController(UsuarioData usuarioData)
        {
            _usuarioData = usuarioData;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string correo, string clave)
        {
            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(clave))
            {
                ViewData["Mensaje"] = "Todos los campos son obligatorios";
                return View();
            }

            // Obtener el usuario por correo
            var usuario = await _usuarioData.ObtenerPorCorreo(correo);
            if (usuario == null)
            {
                ViewData["Mensaje"] = "Correo electrónico no registrado";
                return View();
            }

            // Verificar la contraseña
            if (string.IsNullOrEmpty(usuario.Clave))
            {
                ViewData["Mensaje"] = "Error al recuperar la contraseña del usuario";
                return View();
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(clave, usuario.Clave);
            if (!isPasswordValid)
            {
                ViewData["Mensaje"] = "Contraseña incorrecta";
                return View();
            }

            ViewData["Mensaje"] = null;

            // Aquí guardamos la información de nuestro usuario
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties
            {
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}