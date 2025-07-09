using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using NodeCurrencyConverter.DTOs;
using NodeCurrencyConverter.Entities;
using NodeCurrencyConverter.Infrastructure.Models;
using NodeCurrencyConverter.Infrastructure.RepositoryImplementation;
using System.Text.Json;

namespace NodeCurrencyConverter.Repository.Test
{
    public class RepositoryTest
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IMapper> _mockMapper;
        private readonly string _testFilePath;

        public RepositoryTest()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockMapper = new Mock<IMapper>();
            _testFilePath = Path.GetTempFileName();
        }

        #region GetAllCurrencyExchanges Tests
        [Fact]
        public async Task GetAllCurrencyExchanges_ReadsFileCorrectly()
        {
            // Arrange
            var jsonContent = @"[
            {""From"":""USD"",""To"":""EUR"",""Value"":0.85},
            {""From"":""EUR"",""To"":""GBP"",""Value"":0.9}
            ]";

            await File.WriteAllTextAsync(_testFilePath, jsonContent);
            
            _mockConfiguration.Setup(c => c["CurrencyExchangeFilePath"]).Returns(_testFilePath);

            _mockMapper.Setup(m => m.Map<List<CurrencyExchangeEntity>>(It.IsAny<List<CurrencyExchangeModel>>()))
               .Returns(new List<CurrencyExchangeEntity>
               {
                   new CurrencyExchangeEntity
                   (
                       new CurrencyEntity("USD"), 
                       new CurrencyEntity("EUR"), 
                       0.85m
                   ),
                   new CurrencyExchangeEntity
                   (
                       new CurrencyEntity("EUR"),
                       new CurrencyEntity("GBP"),
                       0.9m
                   )
               });

            var repository = new CurrencyExchangeRepository(_mockConfiguration.Object, _mockMapper.Object);

            // Act
            var result = await repository.GetAllCurrencyExchanges();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("USD", result[0].From.Code);
            Assert.Equal("EUR", result[0].To.Code);
            Assert.Equal(0.85m, result[0].Value);
            Assert.Equal("EUR", result[1].From.Code);
            Assert.Equal("GBP", result[1].To.Code);
            Assert.Equal(0.9m, result[1].Value);

            Dispose();
        }

        [Fact]
        public async Task GetAllCurrencyExchanges_FileNotFound_ThrowsException()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["CurrencyExchangeFilePath"]).Returns("nonexistent.json");
            
            var repository = new CurrencyExchangeRepository(_mockConfiguration.Object, _mockMapper.Object);

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => repository.GetAllCurrencyExchanges());

            Dispose();
        }

        [Fact]
        public async Task GetAllCurrencyExchanges_EmptyFile_ReturnsEmptyList()
        {
            // Arrange
            await File.WriteAllTextAsync(_testFilePath, "[]");

            _mockConfiguration.Setup(c => c["CurrencyExchangeFilePath"]).Returns(_testFilePath);

            _mockMapper.Setup(m => m.Map<List<CurrencyExchangeEntity>>(It.IsAny<List<CurrencyExchangeModel>>()))
               .Returns(new List<CurrencyExchangeEntity>());

            var repository = new CurrencyExchangeRepository(_mockConfiguration.Object, _mockMapper.Object);

            // Act
            var result = await repository.GetAllCurrencyExchanges();

            // Assert
            Assert.Empty(result);

            Dispose();
        }
        #endregion

        #region CreateNewNode Tests
        [Fact]
        public async Task CreateNewNode_MapsAndWritesFileCorrectly()
        {
            // Arrange
            var entities = new List<CurrencyExchangeEntity>
            {
                new CurrencyExchangeEntity(new CurrencyEntity("USD"), new CurrencyEntity("EUR"), 0.85m),
                new CurrencyExchangeEntity(new CurrencyEntity("EUR"), new CurrencyEntity("GBP"), 0.9m)
            };

            var models = new List<CurrencyExchangeModel>
            {
                new CurrencyExchangeModel { From = "USD", To = "EUR", Value = 0.85m },
                new CurrencyExchangeModel { From = "EUR", To = "GBP", Value = 0.9m }
            };

            await File.WriteAllTextAsync(_testFilePath, "[]");

            _mockConfiguration.Setup(c => c["CurrencyExchangeFilePath"]).Returns(_testFilePath);
            _mockMapper.Setup(m => m.Map<List<CurrencyExchangeModel>>(entities)).Returns(models);

            var repository = new CurrencyExchangeRepository(_mockConfiguration.Object, _mockMapper.Object);

            // Act
            await repository.CreateNewConnectionNode(entities);

            // Assert
            var fileContent = await File.ReadAllTextAsync(_testFilePath);

            // Comprobar que el archivo contiene la serialización esperada
            var expectedJson = JsonSerializer.Serialize(models, new JsonSerializerOptions { WriteIndented = true });
            Assert.Equal(expectedJson, fileContent);

            Dispose();
        }

        [Fact]
        public async Task CreateNewNode_FileNotFound_ThrowsException()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["CurrencyExchangeFilePath"]).Returns(_testFilePath);

            if (File.Exists(_testFilePath)) File.Delete(_testFilePath);

            var repository = new CurrencyExchangeRepository(_mockConfiguration.Object, _mockMapper.Object);

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => repository.CreateNewConnectionNode(new List<CurrencyExchangeEntity>()));

            Dispose();
        }
        #endregion

        [Fact]
        public void Dispose()
        {
            if (File.Exists(_testFilePath)) File.Delete(_testFilePath);
        }
    }
}