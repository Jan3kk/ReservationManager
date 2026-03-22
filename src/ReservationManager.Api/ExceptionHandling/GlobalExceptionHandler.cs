using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReservationManager.Application.Exceptions;

namespace ReservationManager.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, IHostEnvironment environment)
    {
        _problemDetailsService = problemDetailsService;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = MapToProblemDetails(exception);

        await _problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception,
        }).ConfigureAwait(false);

        return true;
    }

    private ProblemDetails MapToProblemDetails(Exception exception)
    {
        switch (exception)
        {
            case ValidationException validationException:
                return new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "One or more validation errors occurred.",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Extensions =
                    {
                        ["errors"] = validationException.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray()),
                    },
                };

            case NotFoundException notFound:
                return new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not found",
                    Detail = notFound.Message,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                };

            case KeyNotFoundException keyNotFound:
                return new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not found",
                    Detail = keyNotFound.Message,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                };

            case ConflictException conflict:
                return new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Detail = conflict.Message,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                };

            case ArgumentException argument:
                return new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad request",
                    Detail = argument.Message,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                };

            default:
                return new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred while processing your request.",
                    Detail = _environment.IsDevelopment() ? exception.ToString() : null,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                };
        }
    }
}
