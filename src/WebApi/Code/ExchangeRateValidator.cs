using Entities;
using FluentValidation;

namespace WebApi.Code;

public sealed class ExchangeRateValidator : AbstractValidator<ExchangeRate>
{
    public ExchangeRateValidator()
    {
        RuleFor(x => x.CurrencyCode).NotEmpty().WithMessage("Missing currency code");
        RuleFor(x => x.Value).NotEmpty().WithMessage("Missing currecy value");
        RuleFor(x => x.Value).GreaterThan(0).WithMessage("Currency value can't be lower then 0");
        RuleFor(x => x.Date).NotEmpty().WithMessage("Missing currency exchange rate date");
    }
}
