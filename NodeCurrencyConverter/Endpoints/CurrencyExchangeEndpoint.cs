using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.DTOs;
using NodeCurrencyConverter.Entities;

namespace NodeCurrencyConverter.Api.Endpoints;

public static class CurrencyExchangeEndpoints
{
    public static void MapCurrencyExchangeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api")
            .WithTags("Currency Operations")
            .WithOpenApi();

        group.MapGet("/api/GetAllCurrencies",
            async (ICurrencyExchangeService _service) =>
        Results.Ok(await _service.GetAllCurrencies()))
        .WithName("GetAllCurrencies");

        group.MapGet("/api/GetAllCurrencyExchanges",
            async (ICurrencyExchangeService _service) =>
        Results.Ok(await _service.GetAllCurrencyExchanges()))
        .WithName("GetAllCurrencyExchanges");

        group.MapGet("/api/GetNeighborNodesByCode/{cod}",
            async (string cod, ICurrencyExchangeService _service) =>
        Results.Ok(await _service.GetNeighborNodesByCode(new CurrencyCode(cod))))
        .WithName("GetNeighborNodesByCode");

        group.MapPost("api/GetShortestPath", async (CurrencyExchangeDto request, ICurrencyExchangeService service) =>
        {
            try
            {
                var currencyExchangeEntity = new CurrencyExchangeEntity
                (
                    new CurrencyCode(request.From),
                    new CurrencyCode(request.To),
                    request.Value
                );


                var result = await service.GetShortestPath(currencyExchangeEntity);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("GetShortestPath");
    }
}
