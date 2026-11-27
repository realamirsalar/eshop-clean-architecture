

using E_Shop.Application.Features.Product.Queries;
using E_Shop.Application.Validators.Product;
using FluentValidation.TestHelper;

namespace E_Shop.Application.Tests.Unit.ValidatorTests.Product
{
    public class GetProductByIdQueryValidatorTests
    {
        private readonly GetProductByIdQueryValidator _validator = new();

        [Fact]
        public void Should_NotHaveError_WhenIdIsGreaterThanZero()
        {
            // Arrange
            var query = new GetProductByIdQuery(1);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Should_HaveError_WhenIdIsZeroOrNegative(int invalidId)
        {
            // Arrange
            var query = new GetProductByIdQuery(invalidId);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Product ID must be greater than 0.");
        }
    }
}
