

using E_Shop.Application.Features.Product.Commands;
using FluentValidation;

namespace E_Shop.Application.Validators.Product
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
              .NotEmpty().WithMessage("Name is required.")
              .MinimumLength(2).WithMessage("Product name must be at least 2 characters long.")
              .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

            RuleFor(x => x.Quantity).NotEmpty()
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");

            RuleFor(x => x.Price).NotEmpty()
                .GreaterThan(1000).WithMessage("Price must be greater than 1000.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category ID is required.")
                .GreaterThan(0).WithMessage("Invalid category ID.");
        }
    }
}
