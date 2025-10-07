using Entities;
using FluentValidation;

namespace WebApi.Code;

public sealed class ExchangeRatesValidator : AbstractValidator<List<ExchangeRate>>
{
    public ExchangeRatesValidator()
    {
        RuleForEach(x => x).SetValidator(new ExchangeRateValidator());
    }
}
