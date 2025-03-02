using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prestamo.Entidades
{
    public class ResumenCliente
    {
        public string PrestamosCliente { get; set; } = null!;
        public string PagosClientePendientes { get; set; } = null!;
    }
}
