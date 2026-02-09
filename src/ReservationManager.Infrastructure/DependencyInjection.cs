using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Infrastructure.Messaging.Consumers;
using ReservationManager.Infrastructure.Persistence;
using ReservationManager.Infrastructure.Repositories;

namespace ReservationManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReservationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<IReservationRepository, ReservationRepository>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<ReservationRequestConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
