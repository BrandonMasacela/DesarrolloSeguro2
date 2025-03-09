using System;

namespace Prestamo.Web.Models
{
    public class PrestamoViewModel
    {
        public int IdPrestamo { get; set; }
        public ClienteInfo Cliente { get; set; }
        public decimal MontoPrestamo { get; set; }
        public decimal ValorInteres { get; set; }
        public decimal ValorTotal { get; set; }
        public MonedaInfo Moneda { get; set; }
        public string Estado { get; set; }
        public string FechaCreacion { get; set; }
    }

    public class ClienteInfo
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
    }

    public class MonedaInfo
    {
        public int IdMoneda { get; set; }
        public string Nombre { get; set; }
    }
} 