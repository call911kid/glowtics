using Glowtics.Api.Responses;
using Glowtics.BLL.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Glowtics.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(context, e);
            }
        }

        public async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var message = GetMessage(exception);
            var errors = GetErrors(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse.Failure(message, errors));
        }

        private static HttpStatusCode GetStatusCode(Exception exception) => exception switch
        {
            BadRequestException => HttpStatusCode.BadRequest,
            
            // ValidationException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };

        private string GetMessage(Exception exception) => exception switch
        {
            NotFoundException => exception.Message,
            BadRequestException => exception.Message,
            _ => _env.IsDevelopment() ? exception.Message : "An error occurred while processing your request."
        };

        private static List<string> GetErrors(Exception exception) => exception switch
        {
            
            // ValidationException validationException => validationException.Errors.Select(e => e.ErrorMessage).ToList(),
            _ => new List<string>()
        };
    }
}
