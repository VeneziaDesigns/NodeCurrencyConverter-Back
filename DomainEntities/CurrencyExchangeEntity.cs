namespace NodeCurrencyConverter.Entities
{
    public class CurrencyExchangeEntity
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}
