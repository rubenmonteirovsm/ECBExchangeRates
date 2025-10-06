
using System.Globalization;
using System.Xml;

using Entities;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient(Constants.HttpClient.ECBClientName, clt =>
{
    clt.BaseAddress = new Uri("http://www.ecb.int");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Web API";
        options.Theme = ScalarTheme.Solarized;
        options.DarkMode = true;
    });
}

app.UseHttpsRedirection();

app.MapGet("/ebc/exchanges-rates", async(IHttpClientFactory httpClient) =>
{
    using var client = httpClient.CreateClient(Constants.HttpClient.ECBClientName);
    using var stream = await client.GetStreamAsync("/stats/eurofxref/eurofxref-daily.xml");
    //load XML document
    var document = new XmlDocument();
    document.Load(stream);
    //add namespaces
    var namespaces = new XmlNamespaceManager(document.NameTable);
    namespaces.AddNamespace("ns", "http://www.ecb.int/vocabulary/2002-08-01/eurofxref");
    namespaces.AddNamespace("gesmes", "http://www.gesmes.org/xml/2002-08-01");
    //get daily rates
    var dailyRates = document.SelectSingleNode("gesmes:Envelope/ns:Cube/ns:Cube", namespaces);
    if (!DateTime.TryParseExact(dailyRates.Attributes["time"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var updateDate))
        updateDate = DateTime.UtcNow;

    //add euro with rate 1
    var ratesToEuro = new List<ExchangeRate>
        {
            new ExchangeRate
            {
                CurrencyCode = "EUR",
                Value = 1,
                LastModifiedDate = DateTime.UtcNow
            }
        };

    foreach (XmlNode currency in dailyRates.ChildNodes)
    {
        //get rate
        if (!decimal.TryParse(currency.Attributes["rate"].Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out var currencyRate))
            continue;

        ratesToEuro.Add(new ExchangeRate()
        {
            CurrencyCode = currency.Attributes["currency"].Value,
            Value = currencyRate,
            LastModifiedDate = updateDate
        });
    }

    return ratesToEuro;
})
.WithDisplayName("Get ECB Exchanges Rates")
.WithName("GetECBExchangesRates");

app.MapPost("/ecb/exchanges-rates", (List<ExchangeRate> List) =>
{
    //var data = context.Request.ReadFromJsonAsync<List<ExchangeRate>>();
})
.WithDisplayName("Post ECB Exchanges Rates")
.WithName("PostECBExchangesRates");

await app.RunAsync();
