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
            detail.RuleFor(d => d.Id).GreaterThanOrEqualTo(0).WithMessage("Detail ID cannot be negative.");
            detail.RuleFor(d => d.ItemId).GreaterThan(0).WithMessage("Please select a valid Item.");
            detail.RuleFor(d => d.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            detail.RuleFor(d => d.UnitId).GreaterThan(0).WithMessage("Please select a valid Unit.");
        });

        RuleFor(x => x.Details)
            .Must(details => details == null || details.Where(d => d.Id > 0).Select(d => d.Id).Distinct().Count() == details.Count(d => d.Id > 0))
            .WithMessage("An existing detail can only appear once in an update.");

        RuleFor(x => x.DeletedDetailIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("Deleted detail IDs must be positive.");

        RuleFor(x => x.DeletedDetailIds)
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
            .WithMessage("A deleted detail ID can only appear once.");
    }
}
