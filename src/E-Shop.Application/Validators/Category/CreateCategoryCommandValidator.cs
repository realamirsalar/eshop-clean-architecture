

using E_Shop.Application.Features.Category.Commands;
using FluentValidation;

namespace E_Shop.Application.Validators.Category
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name cannot exceed 150 characters.");
            RuleFor(c => c.DisplayOrder)
                .GreaterThanOrEqualTo(1).WithMessage("Display order must be a non-negative integer or not zero.")
                .LessThan(1000).WithMessage("Display order must be less than 1000");
        }
    }
}
