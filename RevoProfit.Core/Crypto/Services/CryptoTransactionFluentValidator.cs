using FluentValidation;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;

namespace RevoProfit.Core.Crypto.Services;

public class CryptoTransactionFluentValidator : AbstractValidator<CryptoTransaction>, ICryptoTransactionValidator
{
    public CryptoTransactionFluentValidator()
    {
        RuleFor(transaction => transaction.Type)
            .IsInEnum();

        When(transaction => transaction.FeesAmount != 0, () =>
        {
            RuleFor(transaction => transaction.FeesAmount)
                .GreaterThanOrEqualTo(0);

            RuleFor(transaction => transaction.FeesPrice)
                .GreaterThan(0);

            RuleFor(transaction => transaction.FeesSymbol)
                .NotEmpty();
        });

        When(transaction => transaction.Type is CryptoTransactionType.Buy or CryptoTransactionType.Exchange, () =>
        {
            RuleFor(transaction => transaction.BuyAmount)
                .GreaterThan(0);

            RuleFor(transaction => transaction.BuyPrice)
                .GreaterThan(0);

            RuleFor(transaction => transaction.BuySymbol)
                .NotEmpty();
        });

        When(transaction => transaction.Type is CryptoTransactionType.Sell or CryptoTransactionType.Exchange, () =>
        {
            RuleFor(transaction => transaction.SellAmount)
                .GreaterThan(0);

            RuleFor(transaction => transaction.SellPrice)
                .GreaterThan(0);

            RuleFor(transaction => transaction.SellSymbol)
                .NotEmpty();
        });
    }

    public void ValidateAndThrow(CryptoTransaction instance) => DefaultValidatorExtensions.ValidateAndThrow(this, instance);
    public bool IsValid(CryptoTransaction instance) => Validate(instance).IsValid;
}