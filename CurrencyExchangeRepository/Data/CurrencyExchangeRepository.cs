using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.Entities;
using NodeCurrencyConverter.Infrastructure.Models;

namespace NodeCurrencyConverter.Infrastructure.Data;

public class CurrencyExchangeRepository : ICurrencyExchangeRepository
{
    private readonly string _filePath;

    public CurrencyExchangeRepository(IConfiguration configuration)
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration["CurrencyExchangeFilePath"] ?? "Data/CurrencyExchange.json");
    }

    public async Task<List<CurrencyExchangeEntity>> GetAllCurrencyExchanges()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException("CurrencyExchange.json not found.");
        }

        await using var file = File.OpenRead(_filePath);

        var listCurrencyExchangeModel = await JsonSerializer.DeserializeAsync<IEnumerable<CurrencyExchangeModel>>(file);

        if (listCurrencyExchangeModel == null)
        {
            return new List<CurrencyExchangeEntity>(); 
        }

        return listCurrencyExchangeModel.Select(x => new CurrencyExchangeEntity
        {
            From = x.From,
            To = x.To,
            Value = x.Value
        }).ToList();
    }
}
