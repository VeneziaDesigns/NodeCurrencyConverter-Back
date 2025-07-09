using Microsoft.AspNetCore.Mvc;
using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.DTOs;

namespace NodeCurrencyConverter.Api.Endpoints;

public static class CurrencyExchangeEndpoints
{
    public static void MapCurrencyExchangeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api")
            .WithTags("Currency Operations")
            .WithOpenApi();

        group.MapGet("/GetAllCurrencies",
            async (ICurrencyExchangeService _service) =>
        Results.Ok(await _service.GetAllCurrencies()))
            .WithName("GetAllCurrencies");

        group.MapGet("/GetAllCurrencyExchanges",
            async (ICurrencyExchangeService _service) =>
        Results.Ok(await _service.GetAllCurrencyExchanges()))
            .WithName("GetAllCurrencyExchanges");

        group.MapGet("/GetNeighborNodesByCode/{cod}",
            async (string cod, ICurrencyExchangeService _service) =>
        Results.Ok(await _service.GetNeighborNodesByCode(new CurrencyDto(cod))))
            .WithName("GetNeighborNodesByCode");

        group.MapPost("/GetShortestPath", async (CurrencyExchangeDto request, ICurrencyExchangeService service) =>
        {
            var result = await service.GetShortestPath(request);
            return Results.Ok(result);
        })
        .WithName("GetShortestPath");

        group.MapPost("/CreateNewConnectionNode", async ([FromBody] IEnumerable<CurrencyExchangeDto> request, ICurrencyExchangeService service) =>
        {
            await service.CreateNewConnectionNode(request.ToList());
            return Results.Created();
        })
        .WithName("CreateNewConnectionNode");
    }
}
