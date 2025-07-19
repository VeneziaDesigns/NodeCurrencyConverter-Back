using Microsoft.Extensions.DependencyInjection;
using NodeCurrencyConverter.Api.Endpoints;
using NodeCurrencyConverter.Api.Middleware;
using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.DomainService;
using NodeCurrencyConverter.Infrastructure.Mapping;
using NodeCurrencyConverter.Services;
using AutoMapper;
using Serilog;
using NodeCurrencyConverter.Infrastructure.RepositoryImplementation;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configuracion Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();

        var loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddSerilog();
        });

        // Register AutoMapper DI manual
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        }, loggerFactory);

        IMapper mapper = mapperConfig.CreateMapper();

        // Register services
        builder.Services.AddSingleton(mapper);
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

        // Agregar limite de peticiones 
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("fijo", opt =>
            {
                opt.PermitLimit = 240;      
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });
        });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Dev", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
            options.AddPolicy("Prod", policy =>
            {
                policy.WithOrigins("http://adridomain.duckdns.org")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        try
        {
            Log.Information("Starting the application...");

            var app = builder.Build();

            app.UsePathBase("/NodeCurrencyConverter");

            if (app.Environment.IsDevelopment())
                app.UseCors("Dev");
            else
                app.UseCors("Prod");


            // Middleware para registrar solicitudes HTTP
            app.UseHttpLogging();

            // Middleware para manejo global de excepciones 
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRateLimiter();

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
    }
}