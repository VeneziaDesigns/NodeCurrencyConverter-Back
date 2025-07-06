using NodeCurrencyConverter.DTOs;
using NodeCurrencyConverter.Entities;

namespace NodeCurrencyConverter.Contracts
{
    public interface ICurrencyExchangeService
    {
        Task<List<CurrencyDto>> GetAllCurrencies();
        Task<List<CurrencyExchangeDto>> GetAllCurrencyExchanges();
        Task<List<CurrencyDto>> GetNeighborNodesByCode(CurrencyCode cod);
        Task<List<CurrencyExchangeDto>> GetShortestPath(CurrencyExchangeDto currencyExchangeEntity);
        Task CreateNewNode(List<CurrencyExchangeDto> nodeConnections); 
    }
}
