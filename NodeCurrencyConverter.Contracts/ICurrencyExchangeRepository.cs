using NodeCurrencyConverter.Entities;

namespace NodeCurrencyConverter.Contracts
{
    public interface ICurrencyExchangeRepository
    {
        Task<List<CurrencyExchangeEntity>> GetAllCurrencyExchanges();
        Task CreateNewConnectionNode(List<CurrencyExchangeEntity> nodeConnections);
    }
}
