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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Prestamo.Web.Servives;
using System.Security.Cryptography;


namespace Prestamo.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly UsuarioData _usuarioData;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly AuditoriaService _auditoriaService;

        public LoginController(UsuarioData usuarioData, EmailService emailService, IConfiguration configuration, AuditoriaService auditoriaService)
        {
            _usuarioData = usuarioData;
            _emailService = emailService;
            _configuration = configuration;
            _auditoriaService = auditoriaService;
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
                Console.WriteLine($"Tiempo restante: {tiempoRestante}");
                // Asegurar que no se redondee a 0
                if (tiempoRestante > 0)
                {
                    ViewData["Mensaje"] = $"Por favor espere {(int)tiempoRestante} segundos antes de intentar nuevamente.";
                    ViewData["TiempoBloqueado"] = (int)Math.Ceiling(tiempoRestante); // Redondear hacia arriba
                    Console.WriteLine($"Tiempo bloqueado: {tiempoRestante}");
                    Console.WriteLine(ViewData["TiempoBloqueado"]);
                    return View();
                }
                else
                {
                    ViewData["TiempoBloqueado"] = null; // Evitar valores incorrectos
                }
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

            // Generar código de verificación seguro
            var codigoVerificacion = GenerarCodigoVerificacion();
            HttpContext.Session.SetString("CodigoVerificacion", codigoVerificacion);
            HttpContext.Session.SetString("CorreoVerificacion", correo);

            // Enviar código de verificación por correo
            string asunto = "Código de verificación";
            string mensaje = $"Tu código de verificación es: {codigoVerificacion}";
            await _emailService.EnviarCorreoAsync(correo, asunto, mensaje);

            // Aquí guardamos la información de nuestro usuario
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:expires"]));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: credentials
            );

            HttpContext.Session.SetString("Token", new JwtSecurityTokenHandler().WriteToken(token));
            TempData["Token"] = new JwtSecurityTokenHandler().WriteToken(token);
            return RedirectToAction("VerificarCodigo");
        }

        public IActionResult VerificarCodigo()
        {
            ViewData["Token"] = TempData["Token"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerificarCodigoAsync(string codigo)
        {
            var codigoVerificacion = HttpContext.Session.GetString("CodigoVerificacion");
            var correo = HttpContext.Session.GetString("CorreoVerificacion");
            var token = HttpContext.Session.GetString("Token");

            if (codigo == codigoVerificacion)
            {
                var usuario = _usuarioData.ObtenerPorCorreo(correo!).Result;
                // Crear identidad del usuario
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.Rol)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Generar cookie de autenticación
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties { IsPersistent = false }
                );

                // Opcional: Guardar JWT en cookie adicional
                Response.Cookies.Append("access_token", token!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true
                });
                TempData["Token"] = token;
                await _auditoriaService.RegistrarLog(usuario.Correo, "Inicio de sesión", "Inicio de sesión exitoso");
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
            await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        private string GenerarCodigoVerificacion()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return BitConverter.ToUInt32(bytes, 0).ToString("D6");
            }
        }
    }
}