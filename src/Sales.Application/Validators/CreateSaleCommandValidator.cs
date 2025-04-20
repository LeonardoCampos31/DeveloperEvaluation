using FluentValidation;
using Sales.Application.Commands.Sales;
using Sales.Domain.Interfaces;

namespace Sales.Application.Validators;

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.SaleNumber).NotEmpty();
        RuleFor(x => x.SaleDate).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();

        RuleForEach(x => x.Items).SetValidator(new SaleItemInputDtoValidator());
    }
}

public class SaleItemInputDtoValidator : AbstractValidator<CreateSaleCommand.SaleItemInputDto>
{
    public SaleItemInputDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).InclusiveBetween(1, 20)
              .WithMessage("You cannot buy less than 1 or more than 20 pieces of the same item.");
        RuleFor(x => x.UnitPrice).GreaterThan(0);
    }
}