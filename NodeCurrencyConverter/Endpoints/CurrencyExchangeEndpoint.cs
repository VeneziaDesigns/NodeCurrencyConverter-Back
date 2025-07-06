using Microsoft.AspNetCore.Mvc;
using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.DTOs;
using NodeCurrencyConverter.Entities;
using System.Collections.Generic;

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
        Results.Ok(await _service.GetNeighborNodesByCode(new CurrencyCode(cod))))
        .WithName("GetNeighborNodesByCode");

        group.MapPost("/GetShortestPath", async (CurrencyExchangeDto request, ICurrencyExchangeService service) =>
        {
            try
            {
                var result = await service.GetShortestPath(request);

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("GetShortestPath");

        group.MapPost("/CreateNewNode", async ([FromBody] IEnumerable<CurrencyExchangeDto> request, ICurrencyExchangeService service) =>
        {
            try
            {                
                await service.CreateNewNode(request.ToList());

                return Results.Created();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("CreateNewNode");
    }
}
