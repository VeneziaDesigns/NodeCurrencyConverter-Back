using NodeCurrencyConverter.DTOs;

namespace NodeCurrencyConverter.ServiceContracts
{
    public interface ICurrencyExchangeService
    {
        IEnumerable<CurrencyDto> GetAllCurrencies();
        IEnumerable<CurrencyExchangeDto> GetAllCurrencyExchanges();
    }
}
