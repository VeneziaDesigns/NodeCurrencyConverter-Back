namespace NodeCurrencyConverter.Entities
{
    public sealed record CurrencyExchangeEntity
    {
        public CurrencyCode From { get; init; }
        public CurrencyCode To { get; init; }
        public decimal Value { get; init; }

        public CurrencyExchangeEntity(CurrencyCode from, CurrencyCode to, decimal value)
        {
            From = from;
            To = to;
            Value = value;

            if (from == to)
                throw new ArgumentException("Currency exchange must be between different currencies");

            if (value <= 0)
                throw new ArgumentException("Exchange rate must be positive");
        }
    }
}
