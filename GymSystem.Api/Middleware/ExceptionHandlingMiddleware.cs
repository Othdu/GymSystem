using System.Net;
using System.Text.Json;
using GymSystem.Application.Common;
using GymSystem.Domain.Exceptions;

namespace GymSystem.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
         
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
               await WriteErrorAsync(context,HttpStatusCode.BadRequest, ex.Message);

            }catch(NotFoundException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch(ConcurrencyException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.Conflict, ex.Message);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
            
        }

        private static async Task WriteErrorAsync (HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            
            var body = JsonSerializer.Serialize(new {error=message});
            await context.Response.WriteAsync(body);
        }


    }
}
