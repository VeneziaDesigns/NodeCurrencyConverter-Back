﻿using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.Entities;
using NodeCurrencyConverter.Infrastructure.Models;

namespace NodeCurrencyConverter.Infrastructure.RepositoryImplementation;

public class CurrencyExchangeRepository : ICurrencyExchangeRepository
{
    private readonly string _filePath;
    private readonly IMapper _mapper;

    public CurrencyExchangeRepository(IConfiguration configuration, IMapper mapper)
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration["CurrencyExchangeFilePath"] ?? "Data/CurrencyExchange.json");
        _mapper = mapper;
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

        return _mapper.Map<List<CurrencyExchangeEntity>>(listCurrencyExchangeModel);
    }

    public async Task CreateNewConnectionNode(List<CurrencyExchangeEntity> nodeConnectionsEntity)
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("CurrencyExchange.json not found.");

        var nodeConnectionsModel = _mapper.Map<List<CurrencyExchangeModel>>(nodeConnectionsEntity);

        // Serializar y sobrescribir el archivo
        string updatedJson = JsonSerializer.Serialize(nodeConnectionsModel, new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(_filePath, updatedJson);
    }
}
