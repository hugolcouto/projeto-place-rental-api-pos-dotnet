using System;
using Microsoft.Extensions.DependencyInjection;
using PlaceRentalApp.Application.Services;

namespace PlaceRentalApp.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddService();

        return services;
    }

    public static IServiceCollection AddService(this IServiceCollection services)
    {
        services.AddScoped<IPlaceService, PlaceService>();

        return services;
    }
}
