using Claims.Application.Contracts;
using Claims.Application.Services;
using Claims.Application.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Claims.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IValidator<CreateCoverRequest>, CreateCoverRequestValidator>();
        services.AddSingleton<IValidator<CreateClaimRequest>, CreateClaimRequestValidator>();
        services.AddScoped<ClaimsService>();
        services.AddScoped<CoversService>();
        return services;
    }
}
