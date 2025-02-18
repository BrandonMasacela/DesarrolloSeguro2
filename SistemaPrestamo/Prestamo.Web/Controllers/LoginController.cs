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
    public class LoginController : Controller
    {
        private readonly UsuarioData _usuarioData;
        private readonly EmailService _emailService;

        public LoginController(UsuarioData usuarioData, EmailService emailService)
        {
            _usuarioData = usuarioData;
            _emailService = emailService;
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
                ViewData["Mensaje"] = "Usuario no encontrado";
                return View();
            }

            // Verificar si el usuario está bloqueado
            if (usuario.IsLocked)
            {
                if (usuario.LockoutEnd > DateTime.UtcNow)
                {
                    ViewData["Mensaje"] = "Su cuenta está bloqueada. Por favor, contacte al administrador.";
                    return View();
                }
                else
                {
                    // Si el tiempo de bloqueo ha pasado, resetear el estado
                    await _usuarioData.ResetLockout(usuario.IdUsuario);
                }
            }

            // Verificar si hay un bloqueo temporal activo
            if (usuario.LockoutEnd != null && usuario.LockoutEnd > DateTime.UtcNow)
            {
                var tiempoRestante = (usuario.LockoutEnd.Value - DateTime.UtcNow).TotalSeconds;
                ViewData["Mensaje"] = $"Por favor espere {(int)tiempoRestante} segundos antes de intentar nuevamente.";
                ViewData["TiempoBloqueado"] = (int)tiempoRestante;
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
                usuario.FailedAttempts++;
                usuario.LastFailedAttempt = DateTime.UtcNow;

                int tiempoBloqueado = 0;
                if (usuario.FailedAttempts == 3)
                {
                    tiempoBloqueado = 15;
                    usuario.LockoutEnd = DateTime.UtcNow.AddSeconds(tiempoBloqueado);
                }
                else if (usuario.FailedAttempts == 4)
                {
                    tiempoBloqueado = 30;
                    usuario.LockoutEnd = DateTime.UtcNow.AddSeconds(tiempoBloqueado);
                }
                else if (usuario.FailedAttempts >= 5)
                {
                    usuario.IsLocked = true;
                    usuario.LockoutEnd = DateTime.MaxValue;
                    await _usuarioData.UpdateLockoutStatus(usuario);
                    ViewData["Mensaje"] = "Su cuenta ha sido bloqueada permanentemente. Contacte al administrador.";
                    return View();
                }

                await _usuarioData.UpdateLockoutStatus(usuario);

                if (tiempoBloqueado > 0)
                {
                    ViewData["Mensaje"] = "Demasiados intentos fallidos. Su cuenta está bloqueada temporalmente.";
                    ViewData["TiempoBloqueado"] = tiempoBloqueado;
                    return View();
                }

                ViewData["Mensaje"] = $"Contraseña incorrecta. Intentos restantes: {3 - usuario.FailedAttempts}";
                return View();
            }

            // Login exitoso - resetear contadores
            await _usuarioData.ResetLockout(usuario.IdUsuario);

            // Generar código de verificación
            var codigoVerificacion = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("CodigoVerificacion", codigoVerificacion);
            HttpContext.Session.SetString("CorreoVerificacion", correo);

            // Enviar código de verificación por correo
            string asunto = "Código de verificación";
            string mensaje = $"Tu código de verificación es: {codigoVerificacion}";
            await _emailService.EnviarCorreoAsync(correo, asunto, mensaje);

            return RedirectToAction("VerificarCodigo");
        }

        public IActionResult VerificarCodigo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerificarCodigo(string codigo)
        {
            var codigoVerificacion = HttpContext.Session.GetString("CodigoVerificacion");
            var correo = HttpContext.Session.GetString("CorreoVerificacion");

            if (codigo == codigoVerificacion)
            {
                // Obtener el usuario por correo
                var usuario = await _usuarioData.ObtenerPorCorreo(correo);

                // Aquí guardamos la información de nuestro usuario
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Correo),
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
            else
            {
                ViewData["Mensaje"] = "Código de verificación incorrecto";
                return View();
            }
        }

        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}