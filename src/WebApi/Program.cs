
using Dapper;
using Entities;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Scalar.AspNetCore;
using System.Globalization;
using System.Xml;
using WebApi.Code;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient(Constants.HttpClient.ECBClientName, clt =>
{
    clt.BaseAddress = new Uri("http://www.ecb.int");
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(str => true);
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
    });
});
builder.Services.AddScoped<IECBExchangeRateService, ECBExchangeRateService>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Configure Dapper to support DateOnly
SqlMapper.AddTypeHandler(new SqlDateOnlyTypeHandler());

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

//because of devoteam user policies and certificates
//app.UseHttpsRedirection();

app.UseCors();

app.MapGet("/ebc/exchanges-rates", async (IECBExchangeRateService srv, HttpContext ctx) =>
{
    var source = ExchangeRateSource.ECB;

    if (ctx.Request.QueryString.HasValue && Enum.TryParse<ExchangeRateSource>(ctx.Request.Query["source"].ToString(), out var result))
        source = result;

    return await srv.GetExchangerRatesAsync(source);
})
.WithDisplayName("Get ECB Exchanges Rates")
.WithName("GetECBExchangesRates");

app.MapPost("/ecb/exchanges-rates", async (IECBExchangeRateService srv, List<ExchangeRate> List) => await srv.SetExchangeRatesAsync(List))
.WithDisplayName("Post ECB Exchanges Rates")
.WithName("PostECBExchangesRates");

await app.RunAsync();

