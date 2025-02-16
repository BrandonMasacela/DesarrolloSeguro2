using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prestamo.Data;
using Prestamo.Entidades;
using System.Security.Claims;

namespace Prestamo.Web.Controllers
{
    [Authorize]
    public class ClienteController : Controller
    {
        private readonly ClienteData _clienteData;
        private readonly CuentaData _cuentaData;

        public ClienteController(ClienteData clienteData, CuentaData cuentaData)
        {
            _clienteData = clienteData;
            _cuentaData = cuentaData;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<Cliente> lista = await _clienteData.Lista();
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Cliente objeto)
        {
            string respuesta = await _clienteData.Crear(objeto);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] Cliente objeto)
        {
            string respuesta = await _clienteData.Editar(objeto);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int Id)
        {
            string respuesta = await _clienteData.Eliminar(Id);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [Authorize(Roles = "Cliente")]
        public IActionResult Cuenta()
        {
            var idCliente = User.FindFirst("IdCliente")?.Value;
            if (string.IsNullOrEmpty(idCliente))
            {
                return RedirectToAction("Index", "Home");
            }

            var modelo = new CuentaViewModel
            {
                IdCliente = int.Parse(idCliente)
            };

            return View(modelo);
        }

        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> ObtenerCuenta(int idCliente)
        {
            try
            {
                var cuenta = await _cuentaData.ObtenerCuenta(idCliente);
                return Json(new { data = cuenta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = "Error al obtener los datos de la cuenta: " + ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Depositar([FromBody] DepositoRequest request)
        {
            try
            {
                await _cuentaData.Depositar(request.IdCliente, request.Monto);
                return Json(new { data = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = "Error al realizar el depósito: " + ex.Message });
            }
        }
    }

    public class CuentaViewModel
    {
        public int IdCliente { get; set; }
    }

    public class DepositoRequest
    {
        public int IdCliente { get; set; }
        public decimal Monto { get; set; }
    }
}
