using Prestamo.Data;
using Prestamo.Entidades;

namespace Prestamo.Web.Servives
{
    public class AuditoriaService
    {
        private readonly AuditoriaData _auditoriaData;

        public AuditoriaService(AuditoriaData auditoriaData)
        {
            _auditoriaData = auditoriaData;
        }

        public async Task RegistrarLog(string usuario, string accion, string detalles)
        {
            var log = new Auditoria
            {
                Usuario = usuario,
                Accion = accion,
                Fecha = DateTime.UtcNow,
                Detalles = detalles
            };

            await _auditoriaData.Insertar(log);
        }
    }
}