namespace NodeCurrencyConverter.Entities
{
    public sealed record CurrencyExchangeEntity
    {
        public CurrencyEntity From { get; init; }
        public CurrencyEntity To { get; init; }
        public decimal Value { get; init; }

        public CurrencyExchangeEntity(CurrencyEntity from, CurrencyEntity to, decimal value)
        {
            From = from;
            To = to;
            Value = Math.Round(value, 2);

            if (from == to)
                throw new ArgumentException("Currency exchange must be between different currencies");

            if (value <= 0)
                throw new ArgumentException("Exchange rate must be positive");
        }
    }
}
