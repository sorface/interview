using System.Net;
using Ardalis.SmartEnum;
using Interview.Backend.Responses;
using Interview.Domain;

namespace Interview.Backend.Errors;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
            if (httpContext.Response.StatusCode >= 500)
            {
                logger.LogError(ex, "Something went wrong: {Path} {Method}", httpContext.Request.Path, httpContext.Request.Method);
            }
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (code, errorMessage) = GetResponseDetail(exception);
        context.Response.StatusCode = (int)code;
        await context.Response.WriteAsJsonAsync(
            new MessageResponse { Message = errorMessage, },
            context.RequestAborted);

        static (HttpStatusCode Code, string ErrorMessage) GetResponseDetail(Exception exception)
        {
            return exception switch
            {
                AccessDeniedException => (HttpStatusCode.Forbidden, exception.Message),
                NotFoundException => (HttpStatusCode.NotFound, exception.Message),
                UserException or SmartEnumNotFoundException => (HttpStatusCode.BadRequest, exception.Message),
                _ => (HttpStatusCode.InternalServerError, "Internal Server Error."),
            };
        }
    }
}
