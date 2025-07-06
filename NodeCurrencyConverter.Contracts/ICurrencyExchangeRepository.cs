using NodeCurrencyConverter.Entities;

namespace NodeCurrencyConverter.Contracts
{
    public interface ICurrencyExchangeRepository
    {
        Task<List<CurrencyExchangeEntity>> GetAllCurrencyExchanges();
        Task CreateNewNode(List<CurrencyExchangeEntity> nodeConnections);
    }
}
