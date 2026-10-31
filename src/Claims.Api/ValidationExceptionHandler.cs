using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Claims.Api;

internal sealed class ValidationExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validation)
        {
            return false;
        }

        var errors = validation.Errors
            .GroupBy(failure => JsonNamingPolicy.CamelCase.ConvertName(failure.PropertyName))
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());

        logger.LogInformation("Rejected an invalid request: {Fields}", string.Join(", ", errors.Keys));

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new HttpValidationProblemDetails(errors),
            Exception = exception
        });
    }
}
