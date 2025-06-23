using Moq;
using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.DTOs;
using NodeCurrencyConverter.Entities;
using NodeCurrencyConverter.Services;
using System.Reflection;

namespace NodeCurrencyConverter.Service.Test
{
    public class CurrencyExchangeServiceTests
    {
        private readonly Mock<ICurrencyExchangeRepository> _mockRepository;
        private readonly Mock<ICurrencyRepositoryCache> _mockCache;
        private readonly CurrencyExchangeService _service;

        public CurrencyExchangeServiceTests()
        {
            _mockRepository = new Mock<ICurrencyExchangeRepository>();
            _mockCache = new Mock<ICurrencyRepositoryCache>();
            _service = new CurrencyExchangeService(_mockRepository.Object, _mockCache.Object);
        }

        [Fact]
        public async Task GetAllCurrencies_WithFullCache_ReturnsFromCache()
        {
            var cachedCurrencies = new List<CurrencyDto>
            {
                new CurrencyDto { Currency = "USD" },
                new CurrencyDto { Currency = "EUR" }
            };
            _mockCache.Setup(c => c.GetCacheList<CurrencyDto>("currency")).Returns(cachedCurrencies);

            var result = await _service.GetAllCurrencies();

            Assert.Equal(cachedCurrencies.Count, result.Count);
            Assert.Equal(cachedCurrencies[0].Currency, result[0].Currency);
            Assert.Equal(cachedCurrencies[1].Currency, result[1].Currency);
            _mockRepository.Verify(r => r.GetAllCurrencyExchanges(), Times.Never);
        }

        [Fact]
        public async Task GetAllCurrencies_WithEmptyCurrencyCacheButFullExchangeCache_ReturnsFromExchangeCache()
        {
            _mockCache.Setup(c => c.GetCacheList<CurrencyDto>("currency")).Returns(new List<CurrencyDto>());
            var cachedExchanges = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity { From = "USD", To = "EUR", Value = 0.85m },
                new CurrencyExchangeEntity { From = "EUR", To = "GBP", Value = 0.9m }
            };
            _mockCache.Setup(c => c.GetCacheList<CurrencyExchangeEntity>("currencyExchange")).Returns(cachedExchanges);

            var result = await _service.GetAllCurrencies();

            Assert.Equal(3, result.Count);
            Assert.Contains(result, c => c.Currency == "USD");
            Assert.Contains(result, c => c.Currency == "EUR");
            Assert.Contains(result, c => c.Currency == "GBP");
            _mockRepository.Verify(r => r.GetAllCurrencyExchanges(), Times.Never);
            _mockCache.Verify(c => c.SetCacheList("currency", It.IsAny<List<CurrencyDto>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllCurrencies_WithEmptyCaches_ReturnsFromRepository()
        {
            _mockCache.Setup(c => c.GetCacheList<CurrencyDto>("currency")).Returns(new List<CurrencyDto>());
            _mockCache.Setup(c => c.GetCacheList<CurrencyExchangeEntity>("currencyExchange")).Returns(new List<CurrencyExchangeEntity>());
            var repositoryExchanges = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity { From = "USD", To = "EUR", Value = 0.85m },
                new CurrencyExchangeEntity { From = "EUR", To = "GBP", Value = 0.9m }
            };
            _mockRepository.Setup(r => r.GetAllCurrencyExchanges()).ReturnsAsync(repositoryExchanges);

            var result = await _service.GetAllCurrencies();

            Assert.Equal(3, result.Count);
            Assert.Contains(result, c => c.Currency == "USD");
            Assert.Contains(result, c => c.Currency == "EUR");
            Assert.Contains(result, c => c.Currency == "GBP");
            _mockRepository.Verify(r => r.GetAllCurrencyExchanges(), Times.Once);
            _mockCache.Verify(c => c.SetCacheList("currencyExchange", It.IsAny<List<CurrencyExchangeEntity>>()), Times.Once);
            _mockCache.Verify(c => c.SetCacheList("currency", It.IsAny<List<CurrencyDto>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllCurrencyExchanges_WithFullCache_ReturnsFromCache()
        {
            var cachedExchanges = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity { From = "USD", To = "EUR", Value = 0.85m }
            };
            _mockCache.Setup(c => c.GetCacheList<CurrencyExchangeEntity>("currencyExchange")).Returns(cachedExchanges);

            var result = await _service.GetAllCurrencyExchanges();

            Assert.Single(result);
            Assert.Equal("USD", result[0].From);
            Assert.Equal("EUR", result[0].To);
            Assert.Equal(0.85m, result[0].Value);
            _mockRepository.Verify(r => r.GetAllCurrencyExchanges(), Times.Never);
        }

        [Fact]
        public async Task GetAllCurrencyExchanges_WithEmptyCache_ReturnsFromRepository()
        {
            _mockCache.Setup(c => c.GetCacheList<CurrencyExchangeEntity>("currencyExchange")).Returns(new List<CurrencyExchangeEntity>());
            var repositoryExchanges = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity { From = "USD", To = "EUR", Value = 0.85m }
            };
            _mockRepository.Setup(r => r.GetAllCurrencyExchanges()).ReturnsAsync(repositoryExchanges);

            var result = await _service.GetAllCurrencyExchanges();

            Assert.Single(result);
            Assert.Equal("USD", result[0].From);
            Assert.Equal("EUR", result[0].To);
            Assert.Equal(0.85m, result[0].Value);
            _mockCache.Verify(c => c.SetCacheList("currencyExchange", It.IsAny<List<CurrencyExchangeEntity>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllCurrencyExchanges_WithEmptyCacheAndEmptyRepository_ReturnsNull()
        {
            _mockCache.Setup(c => c.GetCacheList<CurrencyExchangeEntity>("currencyExchange")).Returns(new List<CurrencyExchangeEntity>());
            _mockRepository.Setup(r => r.GetAllCurrencyExchanges()).ReturnsAsync(new List<CurrencyExchangeEntity>());

            var result = await _service.GetAllCurrencyExchanges();

            Assert.Null(result);
        }

        [Fact]
        public async Task GetShortestPath_DirectConversion_ReturnsCorrectPath()
        {
            var exchanges = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity { From = "USD", To = "EUR", Value = 0.85m }
            };
            _mockCache.Setup(c => c.GetCacheList<CurrencyExchangeEntity>("currencyExchange")).Returns(exchanges);

            var result = await _service.GetShortestPath("USD", "EUR", 100m);

            Assert.Single(result);
            Assert.Equal("USD", result[0].From);
            Assert.Equal("EUR", result[0].To);
            Assert.Equal(85m, result[0].Value);
        }

        [Fact]
        public async Task GetShortestPath_IndirectConversion_ReturnsCorrectPath()
        {
            var exchanges = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity { From = "USD", To = "EUR", Value = 0.85m },
                new CurrencyExchangeEntity { From = "EUR", To = "GBP", Value = 0.9m }
            };
            _mockCache.Setup(c => c.GetCacheList<CurrencyExchangeEntity>("currencyExchange")).Returns(exchanges);

            var result = await _service.GetShortestPath("USD", "GBP", 100m);

            Assert.Equal(2, result.Count);
            Assert.Equal("USD", result[0].From);
            Assert.Equal("EUR", result[0].To);
            Assert.Equal(85m, result[0].Value);
            Assert.Equal("EUR", result[1].From);
            Assert.Equal("GBP", result[1].To);
            Assert.Equal(76.5m, result[1].Value);
        }

        [Fact]
        public async Task GetShortestPath_NoConversionPath_ThrowsException()
        {
            var exchanges = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity { From = "USD", To = "EUR", Value = 0.85m }
            };
            _mockCache.Setup(c => c.GetCacheList<CurrencyExchangeEntity>("currencyExchange")).Returns(exchanges);

            await Assert.ThrowsAsync<Exception>(() => _service.GetShortestPath("USD", "GBP", 100m));
        }

        [Fact]
        public async Task GetShortestPath_EmptyCache_FetchesFromRepository()
        {
            _mockCache.Setup(c => c.GetCacheList<CurrencyExchangeEntity>("currencyExchange")).Returns(new List<CurrencyExchangeEntity>());
            var repositoryExchanges = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity { From = "USD", To = "EUR", Value = 0.85m }
            };
            _mockRepository.Setup(r => r.GetAllCurrencyExchanges()).ReturnsAsync(repositoryExchanges);

            var result = await _service.GetShortestPath("USD", "EUR", 100m);

            Assert.Single(result);
            Assert.Equal("USD", result[0].From);
            Assert.Equal("EUR", result[0].To);
            Assert.Equal(85m, result[0].Value);
            _mockCache.Verify(c => c.SetCacheList("currencyExchange", It.IsAny<List<CurrencyExchangeEntity>>()), Times.Once);
        }

        [Fact]
        public async Task GetShortestPath_EmptyCacheAndEmptyRepository_ReturnsNull()
        {
            _mockCache.Setup(c => c.GetCacheList<CurrencyExchangeEntity>("currencyExchange")).Returns(new List<CurrencyExchangeEntity>());
            _mockRepository.Setup(r => r.GetAllCurrencyExchanges()).ReturnsAsync(new List<CurrencyExchangeEntity>());

            var result = await _service.GetShortestPath("USD", "EUR", 100m);

            Assert.Null(result);
        }

        // Pruebas para métodos privados usando reflection
        [Fact]
        public void ProcessConversion_ValidPath_ReturnsCorrectConversion()
        {
            var exchanges = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity { From = "USD", To = "EUR", Value = 0.85m },
                new CurrencyExchangeEntity { From = "EUR", To = "GBP", Value = 0.9m }
            };

            var result = InvokePrivateMethod<List<CurrencyExchangeDto>>(_service, "ProcessConversion",
                new object[] { exchanges, "USD", "GBP", 100m });

            Assert.Equal(2, result.Count);
            Assert.Equal("USD", result[0].From);
            Assert.Equal("EUR", result[0].To);
            Assert.Equal(85m, result[0].Value);
            Assert.Equal("EUR", result[1].From);
            Assert.Equal("GBP", result[1].To);
            Assert.Equal(76.5m, result[1].Value);
        }

        [Fact]
        public void BuildGraph_ValidExchanges_ReturnsCorrectGraph()
        {
            var exchanges = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity { From = "USD", To = "EUR", Value = 0.85m },
                new CurrencyExchangeEntity { From = "EUR", To = "GBP", Value = 0.9m }
            };

            var result = InvokePrivateMethod<Dictionary<string, List<(string To, decimal Value)>>>(_service, "BuildGraph",
                new object[] { exchanges });

            Assert.Equal(2, result.Count);
            Assert.Contains("USD", result.Keys);
            Assert.Contains("EUR", result.Keys);
            Assert.Single(result["USD"]);
            Assert.Single(result["EUR"]);
            Assert.Equal("EUR", result["USD"][0].To);
            Assert.Equal(0.85m, result["USD"][0].Value);
            Assert.Equal("GBP", result["EUR"][0].To);
            Assert.Equal(0.9m, result["EUR"][0].Value);
        }

        [Fact]
        public void FindShortestPath_ValidPath_ReturnsCorrectPath()
        {
            var graph = new Dictionary<string, List<(string To, decimal Value)>>
            {
                { "USD", new List<(string, decimal)> { ("EUR", 0.85m) } },
                { "EUR", new List<(string, decimal)> { ("GBP", 0.9m) } }
            };

            var result = InvokePrivateMethod<List<string>>(_service, "FindShortestPath",
                new object[] { graph, "USD", "GBP" });

            Assert.Equal(3, result.Count);
            Assert.Equal("USD", result[0]);
            Assert.Equal("EUR", result[1]);
            Assert.Equal("GBP", result[2]);
        }

        [Fact]
        public void CalculateConversion_ValidPath_ReturnsCorrectConversion()
        {
            var path = new List<string> { "USD", "EUR", "GBP" };
            var exchanges = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity { From = "USD", To = "EUR", Value = 0.85m },
                new CurrencyExchangeEntity { From = "EUR", To = "GBP", Value = 0.9m }
            };

            var result = InvokePrivateMethod<List<CurrencyExchangeDto>>(_service, "CalculateConversion",
                new object[] { path, 100m, exchanges });

            Assert.Equal(2, result.Count);
            Assert.Equal("USD", result[0].From);
            Assert.Equal("EUR", result[0].To);
            Assert.Equal(85m, result[0].Value);
            Assert.Equal("EUR", result[1].From);
            Assert.Equal("GBP", result[1].To);
            Assert.Equal(76.5m, result[1].Value);
        }

        [Fact]
        public void ValidateInformationReceived_WithNullList_ReturnsFalse()
        {
            var result = InvokePrivateMethod<bool>(_service, "ValidateInformationRecived", new Type[] { typeof(object) }, new object[] { null });
            Assert.False(result);
        }

        [Fact]
        public void ValidateInformationReceived_WithEmptyList_ReturnsFalse()
        {
            var result = InvokePrivateMethod<bool>(_service, "ValidateInformationRecived", new Type[] { typeof(object) }, new object[] { new List<object>() });
            Assert.False(result);
        }

        [Fact]
        public void ValidateInformationReceived_WithNonEmptyList_ReturnsTrue()
        {
            var result = InvokePrivateMethod<bool>(_service, "ValidateInformationRecived", new Type[] { typeof(object) }, new object[] { new List<object> { new object() } });
            Assert.True(result);
        }

        // Métodos auxiliar para invocar métodos privados usando reflexion
        private T InvokePrivateMethod<T>(object obj, string methodName, object[] parameters)
        {
            var type = obj.GetType();
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)method.Invoke(obj, parameters);
        }

        private T InvokePrivateMethod<T>(object obj, string methodName, Type[] typeArguments, object[] parameters)
        {
            var type = obj.GetType();
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(typeArguments);
            return (T)genericMethod.Invoke(obj, parameters);
        }

    }
}