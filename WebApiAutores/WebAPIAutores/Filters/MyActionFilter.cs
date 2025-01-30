using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPIAutores.Filters
{
    public class MyActionFilter : IActionFilter
    {
        private readonly ILogger<MyActionFilter> _logger;

        public MyActionFilter(ILogger<MyActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("Antes de ejecutar la accion");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("Despues de ejecutar la accion");
        }

    }
}
