using E_Shop.Application.Features.Category.Commands;
using FluentValidation;


namespace E_Shop.Application.Validators.Category
{
    public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category id is required.")
                .GreaterThan(0).WithMessage("Category id must be greater then 0.");
        }
    }
}
