using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.Entities;

namespace NodeCurrencyConverter.DomainService
{
    public class CurrencyExchangeDomainService : ICurrencyExchangeDomainService
    {
        public List<CurrencyExchangeEntity> GetValidConnections(List<CurrencyExchangeEntity> incomingConnections, List<CurrencyExchangeEntity> existingConnections)
        {
            var validNodeConnections = incomingConnections.Where
            (n => !existingConnections.Contains(n)).ToHashSet().ToList();

            if (validNodeConnections.Count == 0)
                throw new Exception($"New connections between nodes already exist");

            return validNodeConnections;
        }
    }
}
