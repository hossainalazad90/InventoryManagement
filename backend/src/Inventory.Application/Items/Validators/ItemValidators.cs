using FluentValidation;
using Inventory.Application.Items.DTOs;

namespace Inventory.Application.Items.Validators;

public class CreateItemDtoValidator : AbstractValidator<CreateItemDto>
{
    public CreateItemDtoValidator()
    {
        RuleFor(x => x.ItemCode)
            .NotEmpty().WithMessage("Item Code is required.")
            .MaximumLength(50).WithMessage("Item Code must not exceed 50 characters.");

        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage("Item Name is required.")
            .MaximumLength(200).WithMessage("Item Name must not exceed 200 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Please select a valid Category.");

        RuleFor(x => x.UnitId)
            .GreaterThan(0).WithMessage("Please select a valid Unit.");

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder Level cannot be negative.");
    }
}

public class UpdateItemDtoValidator : AbstractValidator<UpdateItemDto>
{
    public UpdateItemDtoValidator()
    {
        RuleFor(x => x.ItemCode)
            .NotEmpty().WithMessage("Item Code is required.")
            .MaximumLength(50).WithMessage("Item Code must not exceed 50 characters.");

        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage("Item Name is required.")
            .MaximumLength(200).WithMessage("Item Name must not exceed 200 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Please select a valid Category.");

        RuleFor(x => x.UnitId)
            .GreaterThan(0).WithMessage("Please select a valid Unit.");

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder Level cannot be negative.");
    }
}
