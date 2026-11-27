

using E_Shop.Application.Features.Product.Commands;
using E_Shop.Application.Validators.Product;
using FluentValidation.TestHelper;

namespace E_Shop.Application.Tests.Unit.ValidatorTests.Product
{
    public class CreateProductCommandValidatorTests
    {
        private readonly CreateProductCommandValidator _validator = new();

        private static CreateProductCommand CreateValidCommand() => new()
        {
            Name = "Wireless Mouse",
            Quantity = 10,
            Price = 50000,
            CategoryId = 1
        };

        [Fact]
        public void Should_NotHaveError_WhenCommandIsValid()
        {
            // Arrange
            var command = CreateValidCommand();

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_HaveError_WhenNameIsEmpty(string? invalidName)
        {
            // Arrange
            var command = CreateValidCommand();
            command.Name = invalidName!;

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Name is required.");
        }
        [Fact]
        public void Should_HaveError_WhenNameIsTooShort()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Name = "A";

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Product name must be at least 2 characters long.");
        }
        [Fact]
        public void Should_HaveError_WhenNameExceedsMaxLength()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Name = new string('A', 101);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Product name cannot exceed 100 characters.");
        }
        [Fact]
        public void Should_HaveError_WhenQuantityIsNegative()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Quantity = -1;

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Quantity)
                  .WithErrorMessage("Quantity cannot be negative.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-500)]
        [InlineData(1000)]
        public void Should_HaveError_WhenPriceIsNotGreaterThan1000(int invalidPrice)
        {
            // Arrange
            var command = CreateValidCommand();
            command.Price = invalidPrice;

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Price)
                  .WithErrorMessage("Price must be greater than 1000.");
        }

        [Fact]
        public void Should_HaveError_WhenCategoryIdIsZero()
        {
            // Arrange
            var command = CreateValidCommand();
            command.CategoryId = 0;

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CategoryId)
                  .WithErrorMessage("Category ID is required.");
        }

        [Fact]
        public void Should_HaveError_WhenCategoryIdIsNegative()
        {
            // Arrange
            var command = CreateValidCommand();
            command.CategoryId = -1;

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CategoryId)
                  .WithErrorMessage("Invalid category ID.");
        }
    }
}
