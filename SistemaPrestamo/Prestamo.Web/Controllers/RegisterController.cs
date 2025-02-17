using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Prestamo.Data;
using Prestamo.Entidades;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prestamo.Web.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UsuarioData _usuarioData;

        public RegisterController(UsuarioData usuarioData)
        {
            _usuarioData = usuarioData;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string nombreCompleto, string correo, string clave)
        {
            if (string.IsNullOrEmpty(nombreCompleto) || string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(clave))
            {
                ViewData["Mensaje"] = "Todos los campos son obligatorios";
                return View();
            }

            if (!Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewData["Mensaje"] = "Correo electrónico no válido";
                return View();
            }

            // Verificar si el correo ya está registrado
            var usuarioExistente = await _usuarioData.ObtenerPorCorreo(correo);
            if (usuarioExistente != null)
            {
                ViewData["Mensaje"] = "El correo electrónico ya está registrado";
                return View();
            }

            // Hashing de la contraseña
            string hashedClave = BCrypt.Net.BCrypt.HashPassword(clave);

            Usuario nuevoUsuario = new Usuario
            {
                NombreCompleto = nombreCompleto,
                Correo = correo,
                Clave = hashedClave,
                Rol = "Administrador" // Asignar rol automáticamente
            };

            bool usuarioCreado = await _usuarioData.Crear(nuevoUsuario);

            if (!usuarioCreado)
            {
                ViewData["Mensaje"] = "Error al crear el usuario";
                return View();
            }

            ViewData["Mensaje"] = "Usuario creado exitosamente";

            // Aquí guardamos la información de nuestro usuario
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nuevoUsuario.NombreCompleto),
                new Claim(ClaimTypes.NameIdentifier, nuevoUsuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, nuevoUsuario.Rol)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties
            {
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
            return RedirectToAction("Index", "Home");
        }
    }
}