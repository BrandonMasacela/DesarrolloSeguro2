using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prestamo.Data;
using Prestamo.Entidades;
using Prestamo.Web.Servives;
using System.Security.Claims;

namespace Prestamo.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ClienteController : Controller
    {
        private readonly ClienteData _clienteData;
        private readonly CuentaData _cuentaData;
        private readonly AuditoriaService _auditoriaService;

        public ClienteController(ClienteData clienteData, CuentaData cuentaData, AuditoriaService auditoriaService)
        {
            _clienteData = clienteData;
            _cuentaData = cuentaData;
            _auditoriaService = auditoriaService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Lista()
        {
            List<Cliente> lista = await _clienteData.Lista();
            await _auditoriaService.RegistrarLog(User.Identity.Name, "Lista", "El usuario solicitó la lista de clientes.");
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear([FromBody] Cliente objeto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string respuesta = await _clienteData.Crear(objeto);
            await _auditoriaService.RegistrarLog(User.Identity.Name, "Crear", $"Cliente creado: {objeto.Nombre} {objeto.Apellido}");
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpPut]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar([FromBody] Cliente objeto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string respuesta = await _clienteData.Editar(objeto);
            await _auditoriaService.RegistrarLog(User.Identity.Name, "Editar", $"Cliente editado: {objeto.Nombre} {objeto.Apellido}");
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpDelete]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int Id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string respuesta = await _clienteData.Eliminar(Id);
            await _auditoriaService.RegistrarLog(User.Identity.Name, "Eliminar", $"Cliente eliminado con ID: {Id}");
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Cuenta()
        {
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            Console.WriteLine(correo);
            if (string.IsNullOrEmpty(correo))
            {
                return RedirectToAction("Index", "Home");
            }

            var cliente = await _clienteData.ObtenerPorCorreo(correo);
            if (cliente == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Cuenta", new { idCliente = cliente.IdCliente });
        }


        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Depositar([FromBody] DepositoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var resultado = await _cuentaData.Depositar(request.IdCliente, request.Monto);
                if (string.IsNullOrEmpty(resultado))
                {
                    await _auditoriaService.RegistrarLog(User.Identity.Name, "Depositar", $"Depósito de {request.Monto} realizado en cuenta de cliente con ID: {request.IdCliente}");
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, error = resultado });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }

    public class DepositoRequest
    {
        public int IdCliente { get; set; }
        public decimal Monto { get; set; }
    }
}
