using Entities;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Xml;
using Dapper;
using FluentValidation;

namespace WebApi.Code;

public sealed class ECBExchangeRateService : IECBExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly IValidator<List<ExchangeRate>> _validator;

    public ECBExchangeRateService(IHttpClientFactory httpClientfactory, IConfiguration config, IValidator<List<ExchangeRate>> validator)
    {
        _httpClientFactory = httpClientfactory;
        _config = config;
        _validator = validator;
    }

    public async ValueTask<List<ExchangeRate>> GetExchangerRatesAsync(ExchangeRateSource exchangeRateSource)
    {
        List<ExchangeRate> ratesToEuro;
        switch (exchangeRateSource)
        {
            case ExchangeRateSource.ECB:
                ratesToEuro = await GetExchangeRatesFromECBAsync();
                break;
            case ExchangeRateSource.DB:
                ratesToEuro = await GetExchangeRatesFromDBAsync();
                break;
            case ExchangeRateSource.Both:
                ratesToEuro = await GetExchangeRatesFromBothSourcesAsync();
                break;
            default:
                ratesToEuro = Enumerable.Empty<ExchangeRate>().ToList();
                break;
        }


        return ratesToEuro;
    }

    private async Task<List<ExchangeRate>> GetExchangeRatesFromBothSourcesAsync()
    {
        throw new NotImplementedException();
    }

    private async Task<List<ExchangeRate>> GetExchangeRatesFromDBAsync()
    {
        throw new NotImplementedException();
    }

    private async Task<List<ExchangeRate>> GetExchangeRatesFromECBAsync()
    {
        using var client = _httpClientFactory.CreateClient(Constants.HttpClient.ECBClientName);
        using var stream = await client.GetStreamAsync("/stats/eurofxref/eurofxref-daily.xml").ConfigureAwait(false);
        //load XML document
        var document = new XmlDocument();
        document.Load(stream);
        //add namespaces
        var namespaces = new XmlNamespaceManager(document.NameTable);
        namespaces.AddNamespace("ns", "http://www.ecb.int/vocabulary/2002-08-01/eurofxref");
        namespaces.AddNamespace("gesmes", "http://www.gesmes.org/xml/2002-08-01");
        //get daily rates
        var dailyRates = document.SelectSingleNode("gesmes:Envelope/ns:Cube/ns:Cube", namespaces);
        if (!DateOnly.TryParseExact(dailyRates!.Attributes!["time"]!.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var updateDate))
            updateDate = DateOnly.FromDateTime(DateTime.UtcNow);

        //add euro with rate 1
        var ratesToEuro = new List<ExchangeRate>
    {
        new ExchangeRate
        {
            CurrencyCode = "EUR",
            Value = 1,
            LastModifiedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Date = updateDate
        }
    };

        foreach (XmlNode currency in dailyRates.ChildNodes)
        {
            //get rate
            if (!decimal.TryParse(currency!.Attributes!["rate"]!.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out var currencyRate))
                continue;

            ratesToEuro.Add(new ExchangeRate()
            {
                CurrencyCode = currency!.Attributes!["currency"]!.Value,
                Value = currencyRate,
                LastModifiedDate = updateDate,
                Date = updateDate
            });
        }

        return ratesToEuro;
    }

    public async ValueTask SetExchangeRatesAsync(List<ExchangeRate> list)
    {
        var validationResult = await _validator.ValidateAsync(list);
        if (validationResult.IsValid)
        {
            var strConn = _config.GetConnectionString("ECB_Database");
            var sql = "INSERT INTO [ExchangeRates]([CurrencyCode],[Value],[Date],[LastModifiedDate]) VALUES(@CurrencyCode,@Value,@Date,@LastModifiedDate)";
            using var conn = new SqlConnection(strConn);
            await conn.OpenAsync().ConfigureAwait(false);
            var rowsAffected = await conn.ExecuteAsync(sql, list).ConfigureAwait(false);
        }
    }
}
