using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.Entities;
using NodeCurrencyConverter.Infrastructure.Models;
using System.Threading;

namespace NodeCurrencyConverter.Infrastructure.RepositoryImplementation;

public class CurrencyExchangeRepository : ICurrencyExchangeRepository
{
    private readonly string _filePath;
    private readonly IMapper _mapper;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public CurrencyExchangeRepository(IConfiguration configuration, IMapper mapper)
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration["CurrencyExchangeFilePath"] ?? "Data/CurrencyExchange.json");
        _mapper = mapper;
    }

    public async Task<List<CurrencyExchangeEntity>> GetAllCurrencyExchanges()
    {
        await _semaphere.WaitAsync();
        try 
	    {	        
		    if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException("CurrencyExchange.json not found.");
            }

            await using var file = File.OpenRead(_filePath);

            var listCurrencyExchangeModel = await JsonSerializer.DeserializeAsync<IEnumerable<CurrencyExchangeModel>>(file);

            return listCurrencyExchangeModel == null ? 
                new List<CurrencyExchangeEntity>()
                : _mapper.Map<List<CurrencyExchangeEntity>>(listCurrencyExchangeModel);
	    }
	    finally
	    {
		    _semaphore.Release();
	    }
    }

    public async Task CreateNewConnectionNode(List<CurrencyExchangeEntity> nodeConnectionsEntity)
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("CurrencyExchange.json not found.");

        var nodeConnectionsModel = _mapper.Map<List<CurrencyExchangeModel>>(nodeConnectionsEntity);

        // Serializar
        string updatedJson = JsonSerializer.Serialize(nodeConnectionsModel, new JsonSerializerOptions { WriteIndented = true });

        // Evita que dos hilos sobrescriban el archivo al mismo tiempo
        await _semaphere.WaitAsync();
        try 
	    {	        
		    await File.WriteAllTextAsync(_filePath, updatedJson);
	    }
	    finally
	    {
		    _semaphore.Release();
	    }
    }
}
