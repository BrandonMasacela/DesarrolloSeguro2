using Microsoft.AspNetCore.Mvc.Filters;

namespace Prestamo.Web.Servives
{
    public class ContentSecurityPolicyFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; frame-ancestors 'self';";
            context.HttpContext.Response.Headers["X-Frame-Options"] = "DENY";
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No se necesita implementar nada aquí
        }
    }
}
