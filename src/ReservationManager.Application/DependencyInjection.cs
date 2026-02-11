using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ReservationManager.Domain.Services;

namespace ReservationManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<AvailabilityService>();

        return services;
    }
}
