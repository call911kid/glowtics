using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Glowtics.Api.Responses;
using Glowtics.BLL.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Glowtics.Api.Middlewares
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        public async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var errorCode = GetErrorCode(exception);
            var message = GetMessage(exception);
            var errors = GetErrors(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse.Failure(errorCode, message, new List<string>(errors)));
        }

        private static string GetErrorCode(Exception exception) => exception switch
        {
            ValidationException => "VALIDATION_ERROR",
            GlowticsException glowticsException => glowticsException.ErrorCode,
            _ => "ERR_INTERNAL_SERVER_ERROR"
        };

        private static HttpStatusCode GetStatusCode(Exception exception) => exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            InvalidCredentialsException => HttpStatusCode.Unauthorized,
            EntityNotFoundException => HttpStatusCode.NotFound,
            BusinessRuleViolationException => HttpStatusCode.BadRequest,
            AccountRestrictedException => HttpStatusCode.Forbidden,
            ExternalServiceException => HttpStatusCode.BadGateway,
            _ => HttpStatusCode.InternalServerError
        };

        private static string GetMessage(Exception exception) => exception switch
        {
            ValidationException => "One or more fields failed validation.",
            GlowticsException glowticsException => glowticsException.Message,
            _ => "An unexpected error occurred."
        };

        private static IEnumerable<string> GetErrors(Exception exception) => exception switch
        {
            ValidationException validationException => validationException.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"),
            GlowticsException glowticsException => glowticsException.Errors,
            _ => Array.Empty<string>()
        };
    }
}
