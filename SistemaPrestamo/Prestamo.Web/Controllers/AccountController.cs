using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prestamo.Data;
using Prestamo.Web.Models;
using Prestamo.Web.Servives;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Prestamo.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountController : Controller
    {
        private readonly UsuarioData _usuarioData;
        private readonly EmailService _emailService;
        private readonly AuditoriaService _auditoriaService;

        public AccountController(UsuarioData usuarioData, EmailService emailService, AuditoriaService auditoriaService)
        {
            _usuarioData = usuarioData;
            _emailService = emailService;
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> SolicitarCodigoVerificacion([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var usuario = await _usuarioData.ObtenerPorId(int.Parse(userId));
            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, usuario.Clave);
            if (!isPasswordValid)
            {
                return Json(new { success = false, message = "La contraseña actual es incorrecta" });
            }

            // Generar código de verificación seguro
            var codigoVerificacion = GenerarCodigoVerificacion();
            HttpContext.Session.SetString("CodigoVerificacion", codigoVerificacion);

            // Enviar código de verificación por correo
            string asunto = "Código de verificación para cambio de contraseña";
            string mensaje = $"Tu código de verificación es: {codigoVerificacion}";
            await _emailService.EnviarCorreoAsync(usuario.Correo, asunto, mensaje);
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var usuario = await _usuarioData.ObtenerPorId(int.Parse(userId));
            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var codigoVerificacion = HttpContext.Session.GetString("CodigoVerificacion");
            if (model.VerificationCode != codigoVerificacion)
            {
                return Json(new { success = false, message = "El código de verificación es incorrecto" });
            }

            usuario.Clave = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _usuarioData.Actualizar(usuario);
            await _auditoriaService.RegistrarLog(User.Identity.Name, "Cambio", $"Contraseña editada: {usuario.Clave}");
            return Json(new { success = true });
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