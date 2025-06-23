using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.DTOs;
using NodeCurrencyConverter.Entities;

namespace NodeCurrencyConverter.Services;

public class CurrencyExchangeService : ICurrencyExchangeService
{
    private readonly ICurrencyExchangeRepository _repository;
    private readonly ICurrencyRepositoryCache _repositoryCache;

    public CurrencyExchangeService(ICurrencyExchangeRepository repository, ICurrencyRepositoryCache repositoryCache)
    {
        _repository = repository;
        _repositoryCache = repositoryCache;
    }

    public async Task<List<CurrencyDto>> GetAllCurrencies()
    {
        List<CurrencyDto> currencyDtoCache = _repositoryCache.GetCacheList<CurrencyDto>("currency");
        
        if (!ValidateInformationRecived<CurrencyDto>(currencyDtoCache))
        {
            List<CurrencyExchangeEntity> currencyExchangeCache = _repositoryCache.GetCacheList<CurrencyExchangeEntity>("currencyExchange");

            if (!ValidateInformationRecived<CurrencyExchangeEntity>(currencyExchangeCache))
            {
                var allCurrencyExchanges = await _repository.GetAllCurrencyExchanges();

                _repositoryCache.SetCacheList("currencyExchange", allCurrencyExchanges);

                currencyExchangeCache = allCurrencyExchanges;
            }

            var currenciesExchangeToDto = currencyExchangeCache
                .SelectMany(e => new[] { e.From, e.To })
                .Distinct()
                .Select(c => new CurrencyDto { Currency = c })
                .ToList();

            _repositoryCache.SetCacheList("currency", currenciesExchangeToDto);

            return currenciesExchangeToDto;
        }

        return currencyDtoCache.Select(c => new CurrencyDto { Currency = c.Currency }).ToList();
    }

    public async Task<List<CurrencyExchangeDto>> GetAllCurrencyExchanges()
    {
        List<CurrencyExchangeEntity> currencyExchangeCache = _repositoryCache.GetCacheList<CurrencyExchangeEntity>("currencyExchange");

        if (ValidateInformationRecived<CurrencyExchangeEntity>(currencyExchangeCache)) 
        {
            return currencyExchangeCache.Select(x => new CurrencyExchangeDto
            {
                From = x.From,
                To = x.To,
                Value = x.Value
            }).ToList();
        }

        List<CurrencyExchangeEntity> allCurrencyExchanges = await _repository.GetAllCurrencyExchanges();

        if (ValidateInformationRecived<CurrencyExchangeEntity>(allCurrencyExchanges))
        {
            _repositoryCache.SetCacheList<CurrencyExchangeEntity>("currencyExchange", allCurrencyExchanges);

            return allCurrencyExchanges.Select(x => new CurrencyExchangeDto
            {
                From = x.From,
                To = x.To,
                Value = x.Value
            }).ToList();
        }

        return null;
    }

    public async Task<List<CurrencyDto>> GetNeighborNodesByCode(string cod)
    {
        List<CurrencyExchangeEntity> currencyExchangeCache = _repositoryCache.GetCacheList<CurrencyExchangeEntity>("currencyExchange");

        if (!ValidateInformationRecived<CurrencyExchangeEntity>(currencyExchangeCache))
        {
            var allCurrencyExchanges = await _repository.GetAllCurrencyExchanges();

            _repositoryCache.SetCacheList("currencyExchange", allCurrencyExchanges);

            currencyExchangeCache = allCurrencyExchanges;
        }

        var neighborCurrencies = currencyExchangeCache
            .Where(x => x.From == cod)
            .Select(x => new CurrencyDto { Currency = x.To })
            .ToList();

        return neighborCurrencies;
    }

    public async Task<List<CurrencyExchangeDto>> GetShortestPath(string from, string to, decimal value)
    {
        var currencyExchangeCache = _repositoryCache.GetCacheList<CurrencyExchangeEntity>("currencyExchange");

        if (!ValidateInformationRecived(currencyExchangeCache))
        {
            var allCurrencyExchanges = await _repository.GetAllCurrencyExchanges();
            
            if (!ValidateInformationRecived(allCurrencyExchanges)) return null;

            _repositoryCache.SetCacheList("currencyExchange", allCurrencyExchanges);
            currencyExchangeCache = allCurrencyExchanges;
        }

        return ProcessConversion(currencyExchangeCache, from, to, value);
    }

    private List<CurrencyExchangeDto> ProcessConversion(List<CurrencyExchangeEntity> currencyExchangeData, string from, string to, decimal value)
    {
        var graph = BuildGraph(currencyExchangeData);

        var path = FindShortestPath(graph, from, to) ?? new List<string>();

        if (path.Count == 0 || path.Count == 1) throw new Exception($"No conversion path found from {from} to {to}");

        return CalculateConversion(path, value, currencyExchangeData);
    }

    private Dictionary<string, List<(string To, decimal Value)>> BuildGraph(List<CurrencyExchangeEntity> exchanges)
    {
        var graph = new Dictionary<string, List<(string To, decimal Value)>>();

        foreach (var exchange in exchanges)
        {
            if (!graph.ContainsKey(exchange.From)) graph[exchange.From] = new List<(string To, decimal Value)>();

            graph[exchange.From].Add((exchange.To, exchange.Value));
        }

        return graph;
    }

    private List<string> FindShortestPath(Dictionary<string, List<(string To, decimal Value)>> graph, string start, string end)
    {
        var queue = new Queue<List<string>>();
        var visited = new HashSet<string>();

        queue.Enqueue(new List<string> { start });

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var currentNode = path.Last();

            if (currentNode == end)
                return path;

            if (!visited.Contains(currentNode))
            {
                visited.Add(currentNode);

                if (graph.ContainsKey(currentNode))
                {
                    foreach (var neighborNode in graph[currentNode])
                    {
                        var newPath = new List<string>(path) { neighborNode.To };
                        queue.Enqueue(newPath);
                    }
                }
            }
        }

        return null;
    }

    private List<CurrencyExchangeDto> CalculateConversion(List<string> path, decimal initialValue, List<CurrencyExchangeEntity> exchanges)
    {
        var results = new List<CurrencyExchangeDto>();
        decimal value = initialValue;

        for (int i = 0; i < path.Count - 1; i++)
        {
            var from = path[i];
            var to = path[i + 1];

            var rate = exchanges.First(e => e.From == from && e.To == to).Value;

            value *= rate;

            results.Add(new CurrencyExchangeDto
            {
                From = from,
                To = to,
                Value = value
            });
        }

        return results;
    }

    private bool ValidateInformationRecived<T>(List<T> genericList)
    {
        if (genericList != null && genericList.Count > 0) return true; 

        return false;
    }
}
