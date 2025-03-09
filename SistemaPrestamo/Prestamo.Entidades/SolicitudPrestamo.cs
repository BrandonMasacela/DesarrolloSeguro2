using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prestamo.Entidades
{
    public class SolicitudPrestamo
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public decimal Monto { get; set; }
        public int Plazo { get; set; } // en meses
        public string? Estado { get; set; } // Pendiente, Aprobado, Rechazado
        public DateTime? FechaSolicitud { get; set; }
        public decimal Sueldo { get; set; }
        public bool EsCasado { get; set; }
        public int NumeroHijos { get; set; }
        public string MetodoPago { get; set; }
        public string Cedula { get; set; }
        public string Ocupacion { get; set; }
    }
}