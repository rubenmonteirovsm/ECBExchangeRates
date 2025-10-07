using Entities;

namespace WebApi.Code
{
    public interface IECBExchangeRateService
    {
        ValueTask<List<ExchangeRate>> GetExchangerRatesAsync(ExchangeRateSource exchangeRateSource);

        ValueTask SetExchangeRatesAsync(List<ExchangeRate> List);
    }
}
