using FluentValidation;
using Inventory.Application.StockTransactions.DTOs;

namespace Inventory.Application.StockTransactions.Validators;

public class CreateStockTransactionValidator : AbstractValidator<CreateStockTransactionDto>
{
    public CreateStockTransactionValidator()
    {
        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction Date is required.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Valid Transaction Type is required.");

        RuleFor(x => x.StoreId)
            .GreaterThan(0).WithMessage("Please select a valid Store.");

        RuleFor(x => x.Details)
            .NotNull().WithMessage("Details are required.")
            .Must(d => d != null && d.Count > 0).WithMessage("At least one detail row is required.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ItemId).GreaterThan(0).WithMessage("Please select a valid Item.");
            detail.RuleFor(d => d.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            detail.RuleFor(d => d.UnitId).GreaterThan(0).WithMessage("Please select a valid Unit.");
        });
    }
}

public class UpdateStockTransactionValidator : AbstractValidator<UpdateStockTransactionDto>
{
    public UpdateStockTransactionValidator()
    {
        RuleFor(x => x.TransactionId)
            .GreaterThan(0).WithMessage("Valid Transaction ID is required.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction Date is required.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Valid Transaction Type is required.");

        RuleFor(x => x.StoreId)
            .GreaterThan(0).WithMessage("Please select a valid Store.");

        RuleFor(x => x.Details)
            .NotNull().WithMessage("Details are required.")
            .Must(d => d != null && d.Count > 0).WithMessage("At least one detail row is required.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ItemId).GreaterThan(0).WithMessage("Please select a valid Item.");
            detail.RuleFor(d => d.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            detail.RuleFor(d => d.UnitId).GreaterThan(0).WithMessage("Please select a valid Unit.");
        });
    }
}
