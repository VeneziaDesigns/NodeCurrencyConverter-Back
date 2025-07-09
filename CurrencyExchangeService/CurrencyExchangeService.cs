using AutoMapper;
using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.DTOs;
using NodeCurrencyConverter.Entities;

namespace NodeCurrencyConverter.Services;

public class CurrencyExchangeService : ICurrencyExchangeService
{
    private readonly ICurrencyExchangeDomainService _domainService;
    private readonly ICurrencyExchangeRepository _repository;
    private readonly ICurrencyRepositoryCache _repositoryCache;
    private readonly IMapper _mapper;

    public CurrencyExchangeService(ICurrencyExchangeDomainService domainService,
        ICurrencyExchangeRepository repository, ICurrencyRepositoryCache repositoryCache,
        IMapper mapper)
    {
        _domainService = domainService;
        _repository = repository;
        _repositoryCache = repositoryCache;
        _mapper = mapper;
    }

    #region public methods
    public async Task<List<CurrencyDto>> GetAllCurrencies()
    {
        var currencies = await GetOrSetCacheAsync(
            "currency",
            async () => await BuildCurrencyListAsync(),
            TimeSpan.FromSeconds(30)
        );

        return _mapper.Map<List<CurrencyDto>>(currencies);
    }

    public async Task<List<CurrencyExchangeDto>> GetAllCurrencyExchanges()
    {
        var currencyExchanges = await GetOrSetCacheAsync(
            "currencyExchange",
            async () => (await _repository.GetAllCurrencyExchanges()),
            TimeSpan.FromSeconds(60)
        );

        return _mapper.Map<List<CurrencyExchangeDto>>(currencyExchanges);
    }

    public async Task<List<CurrencyDto>> GetNeighborNodesByCode(CurrencyDto cod)
    {
        var currencyExchanges = await GetOrSetCacheAsync(
                    "currencyExchange",
                    async () => (await _repository.GetAllCurrencyExchanges()),
                    TimeSpan.FromSeconds(60)
        );

        CurrencyEntity codEntity = _mapper.Map<CurrencyEntity>(cod);

        return currencyExchanges
            .Where(e => e.From.Code.Equals(codEntity.Code))
            .Select(e => new CurrencyDto(e.To.Code))
            .ToList();
    }

    public async Task<List<CurrencyExchangeDto>> GetShortestPath(CurrencyExchangeDto currencyExchangeDto)
    {
        var currencyExchanges = await GetOrSetCacheAsync(
                    "currencyExchange",
                    async () => (await _repository.GetAllCurrencyExchanges()),
                    TimeSpan.FromSeconds(60)
        );

        var currencyExchangeEntity = _mapper.Map<CurrencyExchangeEntity>(currencyExchangeDto);

        return ProcessConversion(_mapper.Map<List<CurrencyExchangeDto>>(currencyExchanges),
            currencyExchangeEntity.From.Code, currencyExchangeEntity.To.Code, currencyExchangeEntity.Value);
    }

    public async Task CreateNewConnectionNode(List<CurrencyExchangeDto> nodeConnections)
    {
        var currencyExchanges = await GetOrSetCacheAsync(
            "currencyExchange",
            async () => (await _repository.GetAllCurrencyExchanges()),
            TimeSpan.FromSeconds(60)
        );

        var validNodeConnections = _domainService.GetValidWithInversesConnections(
            _mapper.Map<List<CurrencyExchangeEntity>>(nodeConnections),
            currencyExchanges);

        currencyExchanges.AddRange(validNodeConnections);

        await _repository.CreateNewConnectionNode(currencyExchanges);
    }
    #endregion

    #region GetShortestPath methods
    private List<CurrencyExchangeDto> ProcessConversion(List<CurrencyExchangeDto> currencyExchangeData, string from, string to, decimal value)
    {
        var graph = BuildGraph(currencyExchangeData);

        var path = FindShortestPath(graph, from, to) ?? new List<string>();

        if (path is null || path.Count < 2) throw new Exception($"No conversion path found from {from} to {to}");

        return CalculateConversion(path, value, currencyExchangeData);
    }

    private Dictionary<string, List<(string To, decimal Value)>> BuildGraph(List<CurrencyExchangeDto> exchanges)
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

    private List<CurrencyExchangeDto> CalculateConversion(List<string> path, decimal initialValue, List<CurrencyExchangeDto> exchanges)
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
            (
                from,
                to,
                value
            ));
        }

        return results;
    }
    #endregion

    #region Cache or validation methods
    private async Task<List<CurrencyEntity>> BuildCurrencyListAsync()
    {
        var currencyExchanges = await GetOrSetCacheAsync(
            "currencyExchange",
            async () => (await _repository.GetAllCurrencyExchanges()),
            TimeSpan.FromSeconds(60)
        );

        return currencyExchanges
            .SelectMany(e => new[] { e.From, e.To })
            .Distinct()
            .ToList();
    }

    private bool ValidateInformationRecived<T>(List<T> genericList)
    {
        if (genericList != null && genericList.Count > 0) return true;

        return false;
    }

    // patrón de caché asíncrono genérico
    private async Task<List<T>> GetOrSetCacheAsync<T>(string cacheKey, Func<Task<List<T>>> fetchData, TimeSpan expiration)
    {
        var cachedData = _repositoryCache.GetCacheList<T>(cacheKey);

        if (cachedData != null && cachedData.Count != 0) return cachedData;

        var data = await fetchData();

        if (data != null && data.Count != 0)
        {
            _repositoryCache.SetCacheList(cacheKey, data, expiration);
        }

        return data ?? new List<T>();
    }
    #endregion
}