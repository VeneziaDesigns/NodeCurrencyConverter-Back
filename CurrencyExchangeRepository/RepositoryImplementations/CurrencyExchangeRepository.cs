using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.Entities;
using NodeCurrencyConverter.Infrastructure.Models;

namespace NodeCurrencyConverter.Infrastructure.RepositoryImplementations;

public class CurrencyExchangeRepository : ICurrencyExchangeRepository
{
    private readonly string _filePath;

    public CurrencyExchangeRepository(IConfiguration configuration)
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration["CurrencyExchangeFilePath"] ?? "Data/CurrencyExchange.json");
    }

    public async Task CreateNewNode(List<CurrencyExchangeEntity> nodeConnectionsEntity)
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("CurrencyExchange.json not found.");

        var nodeConnectionsModel = nodeConnectionsEntity.Select
        (
            n => new CurrencyExchangeModel()
            {
                From = n.From.Code,
                To = n.To.Code,
                Value = n.Value
            }
        );

        // Serializar y sobrescribir el archivo
        string updatedJson = JsonSerializer.Serialize(nodeConnectionsModel, new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(_filePath, updatedJson);
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
        (
            new CurrencyCode(x.From),
            new CurrencyCode(x.To),
            x.Value
        )).ToList();
    }
}
