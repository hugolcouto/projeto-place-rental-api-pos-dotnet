using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlaceRentalApp.API.Persistence;
using PlaceRentalApp.Core.Repositories;
using PlaceRentalApp.Infrastructure.Persistence.Repositories;

namespace PlaceRentalApp.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddData(configuration)
            .AddRepositories();

        return services;
    }

    private static IServiceCollection AddData(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string connectionString = configuration.GetConnectionString("PlaceRentalCs")!;

        //builder.Services.AddDbContext<PlaceRentalDbContext>(
        //    o => o.UseInMemoryDatabase("PlaceRentalDb"));

        services.AddDbContext<PlaceRentalDbContext>(
            o => o.UseSqlServer(connectionString)
        );

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services
            .AddScoped<IPlaceRepository, PlaceRepository>()
            .AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
