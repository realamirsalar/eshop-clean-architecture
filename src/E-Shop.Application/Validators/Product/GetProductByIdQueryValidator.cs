

using E_Shop.Application.Features.Product.Queries;
using FluentValidation;

namespace E_Shop.Application.Validators.Product
{
    public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
    {
        public GetProductByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Product ID must be greater than 0.");
        }
    }
}
