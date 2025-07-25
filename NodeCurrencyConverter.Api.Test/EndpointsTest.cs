using Moq;
using Microsoft.AspNetCore.Http;
using NodeCurrencyConverter.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using NodeCurrencyConverter.DTOs;
using Microsoft.AspNetCore.Mvc;
using NodeCurrencyConverter.Api.Endpoints;
using NodeCurrencyConverter.Entities;
using System.Net.Http.Json;
using System.Net;

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
        public async Task GetAllCurrencies_HandlesException_ReturnsBadRequest()
        {
            // Arrange
            _mockService.Setup(s => s.GetAllCurrencies()).ThrowsAsync(new Exception("Test exception"));

            var handler = (Func<ICurrencyExchangeService, Task<IResult>>)(async (service) =>
            {
                try
                {
                    return Results.Ok(await service.GetAllCurrencies());
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

            // Act
            var result = await handler(_mockService.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequest<string>>(result);
            Assert.Equal("Test exception", badRequestResult.Value);
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
            var request = new CurrencyExchangeDto("USD", "EUR", 100);

            var expectedPath = new List<CurrencyExchangeDto>
            {
                new("USD", "EUR", 85)
            };

            _mockService.Setup(s => s.GetShortestPath(request))
                .ReturnsAsync(expectedPath);

            var handler = (Func<CurrencyExchangeDto, ICurrencyExchangeService, Task<IResult>>)(async (req, service) =>
            {
                var result = await service.GetShortestPath(new CurrencyExchangeDto
                (
                    req.From,
                    req.To, 
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
            var request = new CurrencyExchangeDto("USD", "PES", 100);

            _mockService.Setup(s => s.GetShortestPath(new CurrencyExchangeDto
                (
                    "USD",
                    "PES",
                    100
                )))
                .ThrowsAsync(new Exception("No conversion path found from USD to PES"));

            var handler = (Func<CurrencyExchangeDto, ICurrencyExchangeService, Task<IResult>>)(async (req, service) =>
            {
                try
                {
                    var result = await service.GetShortestPath(new CurrencyExchangeDto
                    (
                        req.From,
                        req.To,
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
            var request = new CurrencyExchangeDto("USD", "USD", 100);     
            
            _mockService.Setup(s => s.GetShortestPath(new CurrencyExchangeDto
                (
                    "USD", 
                    "USD", 
                    100
                )))
                .ThrowsAsync(new ArgumentException("Currency exchange must be between different currencies"));

            var handler = (Func<CurrencyExchangeDto, ICurrencyExchangeService, Task<IResult>>)(async (req, service) =>
            {
                try
                {
                    var result = await service.GetShortestPath(new CurrencyExchangeDto
                        (
                            req.From, 
                            req.To, 
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
            Assert.Equal("Currency exchange must be between different currencies", badRequestResult.Value);
        }

        [Fact]
        public async Task GetNeighborNodesByCode_ReturnsOkResultWithNeighbors()
        {
            // Arrange
            var neighbors = new List<CurrencyDto> { new("EUR"), new("GBP") };
            
            _mockService.Setup(s => s.GetNeighborNodesByCode(It.IsAny<CurrencyDto>())).ReturnsAsync(neighbors);

            var handler = (Func<string, ICurrencyExchangeService, Task<IResult>>)(async (code, service) =>
            {
                return Results.Ok(await service.GetNeighborNodesByCode(new CurrencyDto(code)));
            });

            // Act
            var result = await handler("USD", _mockService.Object);

            // Assert
            var okResult = Assert.IsType<Ok<List<CurrencyDto>>>(result);
            Assert.Equal(neighbors, okResult.Value);
        }


        [Fact]
        public async Task CreateNewNode_ReturnsCreated()
        {
            // Arrange
            var request = new List<CurrencyExchangeDto> 
            {
                new CurrencyExchangeDto("USD", "RUB", 1.2m)
            };

            _mockService.Setup(s => s.CreateNewConnectionNode(It.IsAny<List<CurrencyExchangeDto>>()))
                .Returns(Task.CompletedTask);

            var handler = (Func<List<CurrencyExchangeDto>, ICurrencyExchangeService, Task<IResult>>)(async (req, service) =>
            {
                await service.CreateNewConnectionNode(req);
                return Results.Created();
            });

            // Act
            var result = await handler(request, _mockService.Object);

            // Assert
            var createdResult = Assert.IsType<Created>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }
    }
}