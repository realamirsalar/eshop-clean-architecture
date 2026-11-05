using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace E_Shop.Api.ExceptionHandlers
{

    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(
            exception,
            "An unhandled exception occurred during the processing of the request. MachineName: {MachineName}, TraceId: {TraceId}",
            Environment.MachineName,
            httpContext.TraceIdentifier);

            var statusCode = exception switch
            {
                OperationCanceledException or TaskCanceledException => 499,

                NotImplementedException => StatusCodes.Status501NotImplemented,

                TimeoutException => StatusCodes.Status504GatewayTimeout,

                UnauthorizedAccessException => StatusCodes.Status403Forbidden,

                _ => StatusCodes.Status500InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(statusCode),
                Type = $"https://httpstatuses.com/{statusCode}",
                Detail = statusCode == 500
                    ? "An unexpected error occurred on the server. Our technical team has been notified."
                    : exception.Message,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions.Add("traceId", httpContext.TraceIdentifier);


            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
        private static string GetTitle(int statusCode)
        {
            return statusCode switch
            {
                499 => "Client Closed Request",
                StatusCodes.Status403Forbidden => "Forbidden Access",
                StatusCodes.Status501NotImplemented => "Not Implemented",
                StatusCodes.Status504GatewayTimeout => "Gateway Timeout",
                StatusCodes.Status500InternalServerError => "Server Error",
                _ => "An error occurred"
            };

        }
    }
}
