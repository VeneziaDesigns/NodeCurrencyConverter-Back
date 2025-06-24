using Microsoft.Extensions.Configuration;
using Moq;
using NodeCurrencyConverter.Infrastructure.Data;

namespace NodeCurrencyConverter.Repository.Test
{
    public class RepositoryTest
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly string _testFilePath;

        public RepositoryTest()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _testFilePath = Path.GetTempFileName();
        }

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
            var repository = new CurrencyExchangeRepository(_mockConfiguration.Object);

            // Act
            var result = await repository.GetAllCurrencyExchanges();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("USD", result[0].From.Code);
            Assert.Equal("EUR", result[0].To.Code);
            Assert.Equal(0.85m, result[0].Value);
        }

        [Fact]
        public async Task GetAllCurrencyExchanges_FileNotFound_ThrowsException()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["CurrencyExchangeFilePath"]).Returns("nonexistent.json");
            var repository = new CurrencyExchangeRepository(_mockConfiguration.Object);

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => repository.GetAllCurrencyExchanges());
        }

        [Fact]
        public async Task GetAllCurrencyExchanges_EmptyFile_ReturnsEmptyList()
        {
            // Arrange
            await File.WriteAllTextAsync(_testFilePath, "[]");
            _mockConfiguration.Setup(c => c["CurrencyExchangeFilePath"]).Returns(_testFilePath);
            var repository = new CurrencyExchangeRepository(_mockConfiguration.Object);

            // Act
            var result = await repository.GetAllCurrencyExchanges();

            // Assert
            Assert.Empty(result);
        }

        public void Dispose()
        {
            File.Delete(_testFilePath);
        }
    }
}