using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PlaceRentalApi.Application.Exceptions;

namespace PlaceRentalApi.API.Middlewares;

public class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? details;

        if (exception is NotFoundException)
        {

            details = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = exception.Message
            };

        }
        else
        {
            details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server error"
            };
        }

        // Aqui entra o log de observabilidade
        httpContext.Response.StatusCode = details.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(details, cancellationToken);

        return true;
    }
}
