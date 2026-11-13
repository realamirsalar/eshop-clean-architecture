

using E_Shop.Application.Features.Category.Commands;
using E_Shop.Application.Validators.Category;
using FluentValidation.TestHelper;

namespace E_Shop.Application.Tests.Unit.ValidatorTests.Category
{
    public class CreateCategoryCommandValidatorTests
    {
        private readonly CreateCategoryCommandValidator _validator;

        public CreateCategoryCommandValidatorTests()
        {
            _validator = new CreateCategoryCommandValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void Should_HaveError_WhenNameIsEmptyOrWhiteSpace(string name)
        {
            // Arrange
            var command = new CreateCategoryCommand { Name = name };
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage("Category name is required.");
        }
        [Fact]
        public void Should_HaveError_WhenNameExceedsMaxLength()
        {
            // Arrange
            var command = new CreateCategoryCommand()
            {
                Name = new string('a', 151)
            };
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage("Category name cannot exceed 150 characters.");
        }

        [Theory]
        [InlineData("V")]
        [InlineData("Another Valid Name")]
        public void Should_NotHaveError_WhenNameIsValid(string name)
        {
            // Arrange
            var command = new CreateCategoryCommand { Name = name };
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Name);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Should_HaveError_WhenDisplayOrderIsZeroOrNegative(int displayOrder)
        {
            // Arrange
            var command = new CreateCategoryCommand()
            {
                DisplayOrder = displayOrder
            };
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(c => c.DisplayOrder)
                .WithErrorMessage("Display order must be a positive integer.");
        }
        [Theory]
        [InlineData(1000)]
        [InlineData(1001)]
        [InlineData(20000)]
        public void Should_HaveError_WhenDisplayOrderIsGreaterThanOrEqualTo1000(int displayOrder)
        {
            // Arrange
            var command = new CreateCategoryCommand()
            {
                DisplayOrder = displayOrder
            };
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(c => c.DisplayOrder)
                .WithErrorMessage("Display order must be less than 1000");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void Should_NotHaveError_WhenDisplayOrderIsValid(int displayOrder)
        {
            // Arrange
            var command = new CreateCategoryCommand()
            {
                DisplayOrder = displayOrder
            };
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.DisplayOrder);
        }
    }
}
