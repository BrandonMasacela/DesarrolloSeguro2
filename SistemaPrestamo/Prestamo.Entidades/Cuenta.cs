using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prestamo.Data
{
    public class Cuenta
    {
        public int IdCuenta { get; set; }
        public int IdCliente { get; set; }
        public string Tarjeta { get; set; } = null!;
        public DateTime? FechaCreacion { get; set; }
        public decimal Monto { get; set; }
    }
}
