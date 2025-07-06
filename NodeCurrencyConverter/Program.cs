using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using NodeCurrencyConverter.Api.Endpoints;
using NodeCurrencyConverter.Api.Middleware;
using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.DomainService;
using NodeCurrencyConverter.Infrastructure.RepositoryImplementations;
using NodeCurrencyConverter.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuracion Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) 
    .Enrich.FromLogContext() 
    .CreateLogger();

builder.Host.UseSerilog();

// Register services
builder.Services.AddSingleton<ICurrencyExchangeRepository, CurrencyExchangeRepository>();
builder.Services.AddScoped<ICurrencyExchangeService, CurrencyExchangeService>();
builder.Services.AddScoped<ICurrencyExchangeDomainService, CurrencyExchangeDomainService>();
builder.Services.AddSingleton<ICurrencyRepositoryCache, CurrencyCacheRepository>();

builder.Services.AddMemoryCache(memoryCacheOptions =>
{
    memoryCacheOptions.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
});

// Agregar middleware integrado para registrar solicitudes HTTP
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All; 
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermissiveCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

try
{
    Log.Information("Starting the application...");

    var app = builder.Build();

    app.UseCors("PermissiveCors");

    // Middleware para registrar solicitudes HTTP
    app.UseHttpLogging();

    // Middleware para manejo global de excepciones 
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Registrar endpoints
    app.MapCurrencyExchangeEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "The application failed to start.");
}
finally
{
    Log.CloseAndFlush();
}