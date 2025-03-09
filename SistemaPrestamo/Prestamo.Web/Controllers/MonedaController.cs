using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prestamo.Data;
using Prestamo.Entidades;
using Prestamo.Web.Servives;
using System.Security.Claims;

namespace Prestamo.Web.Controllers
{
    [Authorize]
    public class MonedaController : Controller
    {
        private readonly MonedaData _monedaData;
        private readonly AuditoriaService _auditoriaService;

        public MonedaController(MonedaData monedaData, AuditoriaService auditoriaService)
        {
            _monedaData = monedaData;
            _auditoriaService = auditoriaService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Lista()
        {
            List<Moneda> lista = await _monedaData.Lista();
            await _auditoriaService.RegistrarLog(User.Identity.Name, "Lista", "El usuario solicitó la lista de monedas.");
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear([FromBody] Moneda objeto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string respuesta = await _monedaData.Crear(objeto);
            await _auditoriaService.RegistrarLog(User.Identity.Name, "Crear", $"Moneda creada: {objeto.Nombre}");
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar([FromBody] Moneda objeto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string respuesta = await _monedaData.Editar(objeto);
            await _auditoriaService.RegistrarLog(User.Identity.Name!, "Editar", $"Moneda editada: {objeto.Nombre}");
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int Id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string respuesta = await _monedaData.Eliminar(Id);
            await _auditoriaService.RegistrarLog(User.Identity.Name, "Eliminar", $"Moneda eliminada con ID: {Id}");
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }
    }
}