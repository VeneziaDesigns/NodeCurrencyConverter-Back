namespace NodeCurrencyConverter.DTOs
{
    public record CurrencyExchangeDto
    (
        string From,
        string To,
        decimal Value
    );
}
