using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace E_Shop.Api.ExceptionHandlers
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment env,
            IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _env = env;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (httpContext.Response.HasStarted)
            {
                _logger.LogWarning(exception, "Response has already started, cannot write ProblemDetails.");
                return false;
            }

            var (statusCode, title, detail) = MapException(exception);
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            LogException(exception, statusCode, traceId);

            httpContext.Response.StatusCode = statusCode;

            if (statusCode == 499)
            {
                return true;
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = $"https://httpstatuses.com/{statusCode}",
                Detail = _env.IsDevelopment() ? exception.Message : detail,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = traceId;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
        }

        private static (int StatusCode, string Title, string Detail) MapException(Exception exception) =>
            exception switch
            {
                OperationCanceledException => (499, "Client Closed Request", "The request was canceled by the client."),
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden", "You do not have permission to access this resource."),
                TimeoutException => (StatusCodes.Status504GatewayTimeout, "Gateway Timeout", "The request timed out. Please try again later."),
                NotImplementedException => (StatusCodes.Status501NotImplemented, "Not Implemented", "This feature is not implemented."),
                _ => (StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred on the server.")
            };

        private void LogException(Exception exception, int statusCode, string traceId)
        {
            if (statusCode >= 500)
            {
                _logger.LogError(
                    exception,
                    "Unhandled server exception. StatusCode: {StatusCode}, TraceId: {TraceId}, MachineName: {MachineName}",
                    statusCode, traceId, Environment.MachineName);
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "Client or request exception. StatusCode: {StatusCode}, TraceId: {TraceId}",
                    statusCode, traceId);
            }
        }
    }
}
