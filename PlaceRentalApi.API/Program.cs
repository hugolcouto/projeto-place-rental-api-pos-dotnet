using Microsoft.EntityFrameworkCore;
using PlaceRentalApi.API.Middlewares;
using PlaceRentalApi.API.Models;
using PlaceRentalApi.API.Persistence;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<PlaceRentalDbContext>(
            o => o.UseInMemoryDatabase("PlaceRentalDb")
        );
        // builder.Services.AddSingleton<PlaceRentalDbContext>();

        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddOpenApi();

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();

        var app = builder.Build();

        app.UseExceptionHandler();
        app.MapControllers();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.Run();
    }
}