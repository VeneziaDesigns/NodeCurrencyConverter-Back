using Moq;
using Microsoft.AspNetCore.Http;
using NodeCurrencyConverter.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using NodeCurrencyConverter.DTOs;
using Microsoft.AspNetCore.Mvc;
using NodeCurrencyConverter.Api.Endpoints;
using NodeCurrencyConverter.Entities;

namespace NodeCurrencyConverter.Api.Test
{
    public class EndpointsTests
    {
        private readonly Mock<ICurrencyExchangeService> _mockService;

        public EndpointsTests()
        {
            _mockService = new Mock<ICurrencyExchangeService>();
        }

        [Fact]
        public async Task GetAllCurrencies_ReturnsOkResultWithCurrencies()
        {
            // Arrange
            var currencies = new List<CurrencyDto>
            {
                new("USD"),
                new("EUR")
            };

            _mockService.Setup(s => s.GetAllCurrencies()).ReturnsAsync(currencies);

            // Simula el delegado que se pasa a MapGet
            var handler = (Func<ICurrencyExchangeService, Task<IResult>>)(async (service) =>
            {
                return Results.Ok(await service.GetAllCurrencies());
            });

            // Act
            var result = await handler(_mockService.Object);

            // Assert
            Assert.IsType<Ok<List<CurrencyDto>>>(result);
            var okResult = (Ok<List<CurrencyDto>>)result;
            Assert.Equal(currencies, okResult.Value);
        }

        [Fact]
        public async Task GetAllCurrencies_HandlesEmptyList()
        {
            // Arrange
            var currencies = new List<CurrencyDto>();
            _mockService.Setup(s => s.GetAllCurrencies()).ReturnsAsync(currencies);

            // Simula el delegado que se pasa a MapGet
            var handler = (Func<ICurrencyExchangeService, Task<IResult>>)(async (service) =>
            {
                return Results.Ok(await service.GetAllCurrencies());
            });

            // Act
            var result = await handler(_mockService.Object);

            // Assert
            Assert.IsType<Ok<List<CurrencyDto>>>(result);
            var okResult = (Ok<List<CurrencyDto>>)result;
            Assert.Empty(okResult.Value);
        }

        [Fact]
        public async Task GetAllCurrencies_HandlesException()
        {
            // Arrange
            _mockService.Setup(s => s.GetAllCurrencies()).ThrowsAsync(new Exception("Test exception"));

            // Simula el delegado que se pasa a MapGet
            var handler = (Func<ICurrencyExchangeService, Task<IResult>>)(async (service) =>
            {
                return Results.Ok(await service.GetAllCurrencies());
            });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => handler(_mockService.Object));
        }
        
        [Fact]
        public async Task GetAllCurrencyExchanges_ReturnsOkResultWithCurrencyExchanges()
        {
            // Arrange
            var currencyExchange = new List<CurrencyExchangeDto>
            {
                new("USD", "JPY", 154.44m),
                new("EUR", "GBP", 0.83m)
            };

            _mockService.Setup(s => s.GetAllCurrencyExchanges()).ReturnsAsync(currencyExchange);

            // Simula el delegado que se pasa a MapGet
            var handler = (Func<ICurrencyExchangeService, Task<IResult>>)(async (service) =>
            {
                return Results.Ok(await service.GetAllCurrencyExchanges());
            });

            // Act
            var result = await handler(_mockService.Object);

            // Assert
            Assert.IsType<Ok<List<CurrencyExchangeDto>>>(result);
            var okResult = (Ok<List<CurrencyExchangeDto>>)result;
            Assert.Equal(currencyExchange, okResult.Value);
        }

        [Fact]
        public async Task GetAllCurrencyExchanges_HandlesEmptyList()
        {
            // Arrange
            var currencyExchange = new List<CurrencyExchangeDto>();
            _mockService.Setup(s => s.GetAllCurrencyExchanges()).ReturnsAsync(currencyExchange);

            // Simula el delegado que se pasa a MapGet
            var handler = (Func<ICurrencyExchangeService, Task<IResult>>)(async (service) =>
            {
                return Results.Ok(await service.GetAllCurrencyExchanges());
            });

            // Act
            var result = await handler(_mockService.Object);

            // Assert
            Assert.IsType<Ok<List<CurrencyExchangeDto>>>(result);
            var okResult = (Ok<List<CurrencyExchangeDto>>)result;
            Assert.Empty(okResult.Value);
        }

        [Fact]
        public async Task GetAllCurrencyExchanges_HandlesException()
        {
            // Arrange
            _mockService.Setup(s => s.GetAllCurrencyExchanges()).ThrowsAsync(new Exception("Test exception"));

            // Simula el delegado que se pasa a MapGet
            var handler = (Func<ICurrencyExchangeService, Task<IResult>>)(async (service) =>
            {
                return Results.Ok(await service.GetAllCurrencyExchanges());
            });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => handler(_mockService.Object));
        }

        [Fact]
        public async Task GetShortestPath_ReturnsOkResultWithCorrectPath()
        {
            // Arrange
            var request = new CurrencyExchangeDto("usd", "eur", 100);
            var expectedPath = new List<CurrencyExchangeDto>
            {
                new("USD", "EUR", 85)
            };
            _mockService.Setup(s => s.GetShortestPath(new CurrencyExchangeEntity
                (
                    new CurrencyCode("USD"),
                    new CurrencyCode("EUR"), 
                    100
                )))
                .ReturnsAsync(expectedPath);

            var handler = (Func<CurrencyExchangeDto, ICurrencyExchangeService, Task<IResult>>)(async (req, service) =>
            {
                var result = await service.GetShortestPath(new CurrencyExchangeEntity
                (
                    new CurrencyCode(req.From),
                    new CurrencyCode(req.To), 
                    req.Value
                ));

                return Results.Ok(result);
            });

            // Act
            var result = await handler(request, _mockService.Object);

            // Assert
            var okResult = Assert.IsType<Ok<List<CurrencyExchangeDto>>>(result);
            Assert.Equal(expectedPath, okResult.Value);
        }

        [Fact]
        public async Task GetShortestPath_NoPathExists_ThrowsException()
        {
            // Arrange
            var request = new CurrencyExchangeDto("usd", "pes",100 );
            _mockService.Setup(s => s.GetShortestPath(new CurrencyExchangeEntity
                (
                    new CurrencyCode("USD"),
                    new CurrencyCode("PES"),
                    100
                )))
                .ThrowsAsync(new Exception("No conversion path found from USD to PES"));

            var handler = (Func<CurrencyExchangeDto, ICurrencyExchangeService, Task<IResult>>)(async (req, service) =>
            {
                try
                {
                    var result = await service.GetShortestPath(new CurrencyExchangeEntity
                    (
                        new CurrencyCode(req.From),
                        new CurrencyCode(req.To),
                        req.Value
                    ));
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

            // Act
            var result = await handler(request, _mockService.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequest<string>>(result);
            Assert.Equal("No conversion path found from USD to PES", badRequestResult.Value);
        }

        [Fact]
        public async Task GetShortestPath_SameCurrency_ThrowsException()
        {
            // Arrange
            var request = new CurrencyExchangeDto("usd", "usd", 100);      
            _mockService.Setup(s => s.GetShortestPath(new CurrencyExchangeEntity
                (
                    new CurrencyCode("USD"), 
                    new CurrencyCode("USD"), 
                    100
                )))
                .ThrowsAsync(new Exception("No conversion path found from USD to USD"));

            var handler = (Func<CurrencyExchangeDto, ICurrencyExchangeService, Task<IResult>>)(async (req, service) =>
            {
                try
                {
                    var result = await service.GetShortestPath(new CurrencyExchangeEntity
                        (
                            new CurrencyCode(req.From), 
                            new CurrencyCode(req.To), 
                            req.Value
                        ));
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

            // Act
            var result = await handler(request, _mockService.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequest<string>>(result);
            Assert.Equal("No conversion path found from USD to USD", badRequestResult.Value);
        }
    }
}