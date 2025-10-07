using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Web.Api.GeneratedCode;
using Web.Api.GeneratedCode.Contracts;

namespace WebApp.Pages;

public partial class Index
{
    [Inject]
    public IApiClient? Api { get; set; }

    public QuickGrid<ExchangeRate>? GrdExchangeRates { get; set; }

    protected IQueryable<ExchangeRate>? items;

    public async Task SetExchangesRatesAsync()
    {
        var items = GrdExchangeRates!.Items;

        var response = await Api!.PostECBExchangesRatesAsync(items!).ConfigureAwait(false);
    }

    public async Task GetExchangesRatesAsync(string source)
    {
        var response = await Api!.GetECBExchangesRatesAsync(source).ConfigureAwait(false);

        if (response.IsSuccessful)
        {
            items = response.Content.AsQueryable();
            await GrdExchangeRates!.RefreshDataAsync().ConfigureAwait(false);
        }
    }
}

public enum ExchangeRateSource
{
    ECB,
    DB,
    Both
}