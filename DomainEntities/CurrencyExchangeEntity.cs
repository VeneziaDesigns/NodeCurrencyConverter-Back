namespace NodeCurrencyConverter.Entities
{
    public sealed record CurrencyExchangeEntity
    {
        public CurrencyCode From { get; init; }
        public CurrencyCode To { get; init; }
        public decimal Value { get; init; }

        public CurrencyExchangeEntity(CurrencyCode from, CurrencyCode to, decimal value)
        {
            if (value <= 0)
                throw new ArgumentException("Exchange rate must be positive");

            From = from;
            To = to;
            Value = value;
        }
    }
}
