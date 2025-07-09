using NodeCurrencyConverter.Entities;

namespace NodeCurrencyConverter.Contracts
{
    public interface ICurrencyExchangeDomainService
    {
        List<CurrencyExchangeEntity> GetValidWithInversesConnections(
            List<CurrencyExchangeEntity> incomingConnections,
            List<CurrencyExchangeEntity> existingConnections);
    }
}
