using KO.BuildingBlocks.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Wallet.Api.ExceptionHandler;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
  private readonly ILogger<GlobalExceptionHandler> _logger;

  public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
  {
    _logger = logger;
  }

  public async ValueTask<bool> TryHandleAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken)
  {
    _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", httpContext.TraceIdentifier);

    var statusCode = exception switch
    {
      ArgumentException => (int)HttpStatusCode.BadRequest,
      KeyNotFoundException => (int)HttpStatusCode.NotFound,
      UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
      ValidationException => (int)HttpStatusCode.BadRequest,
      _ => (int)HttpStatusCode.InternalServerError
    };

    var problemDetails = new ProblemDetails
    {
      Status = statusCode,
      Title = GetTitle(statusCode),
      Instance = httpContext.Request.Path
    };

    if (statusCode == 500)
    {
      problemDetails.Detail = "An unexpected error occurred.";
    }
    else
    {
      if (exception is ValidationException validationException)
      {
        problemDetails.Detail = string.Join(", ", validationException.Errors);
      }
      else
      {
        problemDetails.Detail = exception.Message;
      }
    }

    problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

    httpContext.Response.StatusCode = statusCode;
    httpContext.Response.ContentType = "application/problem+json";

    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    return true;
  }

  private static string GetTitle(int statusCode) => statusCode switch
  {
    400 => "Bad Request",
    401 => "Unauthorized",
    404 => "Not Found",
    500 => "Server Error",
    _ => "Error"
  };
}
