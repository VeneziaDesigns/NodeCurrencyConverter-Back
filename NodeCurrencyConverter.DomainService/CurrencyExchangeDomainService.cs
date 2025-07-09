using NodeCurrencyConverter.Contracts;
using NodeCurrencyConverter.Entities;

namespace NodeCurrencyConverter.DomainService
{
    public class CurrencyExchangeDomainService : ICurrencyExchangeDomainService
    {
        public List<CurrencyExchangeEntity> GetValidWithInversesConnections(List<CurrencyExchangeEntity> incomingConnections, List<CurrencyExchangeEntity> existingConnections)
        {
            incomingConnections.AddRange(GetInversesConnections(incomingConnections));

            var validNodeConnections = incomingConnections
                .Where(n => !existingConnections.Any(e =>
                    e.From.Code == n.From.Code && e.To.Code == n.To.Code))
                .GroupBy(n => new { From = n.From.Code, To = n.To.Code })
                .Select(g => g.First())
                .ToList();

            if (validNodeConnections.Count == 0)
                throw new Exception($"New connections between nodes already exist");

            return validNodeConnections;
        }

        private List<CurrencyExchangeEntity> GetInversesConnections(List<CurrencyExchangeEntity> incomingConnections)
        {
            List<CurrencyExchangeEntity> inversesConections = new();

            foreach (var conn in incomingConnections)
            {
                inversesConections.Add
                (
                    new CurrencyExchangeEntity
                    (
                        conn.To,
                        conn.From,
                        1 / conn.Value
                    )
                );
            }

            return inversesConections;
        }
    }
}
