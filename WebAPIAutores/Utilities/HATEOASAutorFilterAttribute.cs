using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebAPIAutores.DTOs;
using WebAPIAutores.Services;

namespace WebAPIAutores.Utilities
{
    public class HATEOASAutorFilterAttribute : HATEOASFilterAttribute
    {
        private readonly GeneradorEnlaces _generadorEnlaces;

        public HATEOASAutorFilterAttribute(GeneradorEnlaces generadorEnlaces)
        {
            _generadorEnlaces = generadorEnlaces;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            var debeIncluir = DebeIncluirHATEOAS(context);
            
            if (!debeIncluir)
            {
                await next();
                return;
            }

            var resultado = context.Result as ObjectResult;
            
            var autorDTO = resultado.Value as AutorDTO;
            if (autorDTO == null)
            {
                var autoresDTO = resultado.Value as List<AutorDTO> ?? throw new
                    ArgumentException("Se esperaba una instancia de AutorDTO o List<AutorDTO>");

                autoresDTO.ForEach(async autor => await _generadorEnlaces.GenerarEnlaces(autor));

                resultado.Value = autoresDTO;
            }
            else
            {
                await _generadorEnlaces.GenerarEnlaces(autorDTO);
            }

            await next();
        }
    }
}
