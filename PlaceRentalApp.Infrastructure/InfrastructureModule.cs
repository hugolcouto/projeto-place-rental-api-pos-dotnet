using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PlaceRentalApp.API.Persistence;
using PlaceRentalApp.Core.Repositories;
using PlaceRentalApp.Infrastructure.Auth;
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
            .AddRepositories()
            .AddAuth(configuration);

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

    private static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        JwtKeyHelper.GetSigningKeyBytes(configuration)
                    )
                };
            });

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
