using NodeCurrencyConverter.DTOs;

namespace NodeCurrencyConverter.Contracts
{
    public interface ICurrencyExchangeService
    {
        Task<List<CurrencyDto>> GetAllCurrencies();
        Task<List<CurrencyExchangeDto>> GetAllCurrencyExchanges();
        Task<List<CurrencyExchangeDto>> GetShortestPath(string from, string to, decimal value);    
    }
}
