namespace WebAPIAutores.Middlewares
{
    public static class LogHTTPResponseMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogHTTPResponse(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LogHTTPResponseMiddleware>();
        }
    }

    public class LogHTTPResponseMiddleware
    {

        private readonly RequestDelegate _siguiente;
        private readonly ILogger<LogHTTPResponseMiddleware> _logger;

        public LogHTTPResponseMiddleware(RequestDelegate siguiente, 
            ILogger<LogHTTPResponseMiddleware> logger)
        {
            _siguiente = siguiente;
            _logger = logger;
        }

        // Invoke o InvokeAsync
        public async Task InvokeAsync(HttpContext context)
        {
            // Crea un MemoryStream que actuará como un buffer temporal para capturar la respuesta HTTP.
            using (var ms = new MemoryStream())
            {
                // Guarda el flujo original de la respuesta para restaurarlo más tarde.
                var originalBodyStream = context.Response.Body;

                // Redirige el flujo de respuesta HTTP al MemoryStream.
                context.Response.Body = ms;

                // Llama al siguiente middleware en la tubería de procesamiento.
                // Esto permite que el middleware posterior manipule la solicitud y la respuesta.
                await _siguiente(context);

                // Establece la posición del MemoryStream al inicio para poder leer desde el principio.
                ms.Seek(0, SeekOrigin.Begin);

                // Lee el contenido de la respuesta capturada en el MemoryStream como una cadena.
                string respuesta = new StreamReader(ms).ReadToEnd();

                // Vuelve a establecer la posición del MemoryStream al inicio para copiar su contenido.
                ms.Seek(0, SeekOrigin.Begin);

                // Copia el contenido del MemoryStream de vuelta al flujo original de respuesta.
                await ms.CopyToAsync(originalBodyStream);

                // Restaura el flujo original de respuesta.
                context.Response.Body = originalBodyStream;

                // Registra el contenido de la respuesta en el log para propósitos de monitoreo o depuración.
                _logger.LogInformation(respuesta);
            }
        }
    }
}
