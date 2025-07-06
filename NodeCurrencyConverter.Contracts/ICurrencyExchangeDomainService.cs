using NodeCurrencyConverter.Entities;

namespace NodeCurrencyConverter.Contracts
{
    public interface ICurrencyExchangeDomainService
    {
        List<CurrencyExchangeEntity> GetValidConnections(
            List<CurrencyExchangeEntity> incomingConnections,
            List<CurrencyExchangeEntity> existingConnections);
    }
}
