using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace NewsApi.CustomMiddleware
{
    /// <summary>
    /// Middleware for handling exceptions during HTTP request processing.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware component in the pipeline.</param>
        /// <param name="logger">The logger for logging exception details.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Handles exceptions that occur during the execution of the HTTP pipeline.
        /// </summary>
        /// <param name="context">The HTTP context of the current request.</param>
        /// <returns>
        /// A task representing the asynchronous operation. Handles exceptions and ensures
        /// proper logging and response formatting.
        /// </returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Proceed to the next middleware
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = "An error occurred while processing your request.",
                Detail = exception.Message
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            return context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
