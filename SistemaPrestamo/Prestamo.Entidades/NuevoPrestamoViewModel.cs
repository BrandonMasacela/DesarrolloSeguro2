using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prestamo.Entidades
{
    public class NuevoPrestamoViewModel
    {
        public int IdSolicitud { get; set; }
        public decimal Monto { get; set; }
        public int Plazo { get; set; }
        public string MetodoPago { get; set; }
        public int IdUsuario { get; set; }
        public string NumeroDocumento { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
    }
}
