

using E_Shop.Application.Features.Category.Commands;
using E_Shop.Application.Validators.Category;
using FluentValidation.TestHelper;

namespace E_Shop.Application.Tests.Unit.ValidatorTests.Category
{
    public class UpdateCategoryCommandValidatorTests
    {
        private readonly UpdateCategoryValidator _validator;

        public UpdateCategoryCommandValidatorTests()
        {
            _validator = new UpdateCategoryValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Should_HaveError_WhenIdIsLessThanOrEqualToZero(int id)
        {
            // Arrange
            var command = new UpdateCategoryCommand { Id = id };
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id)
              .WithErrorMessage("Category Id must be greater than 0.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Id_Is_Valid()
        {
            // Arrange
            var command = new UpdateCategoryCommand { Id = 1 };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")] 
        public void Should_Have_Error_When_Name_Is_Empty_Or_Whitespace(string invalidName)
        {
            // Arrange
            var command = new UpdateCategoryCommand { Name = invalidName };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Category name is required.");
        }

        [Fact]
        public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
        {
            // Arrange
            var command = new UpdateCategoryCommand { Name = new string('A', 101) };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Category name cannot exceed 100 characters.");
        }

        [Theory]
        [InlineData("Electronics")]
        [InlineData("A")] 
        public void Should_Not_Have_Error_When_Name_Is_Valid(string validName)
        {
            // Arrange
            var command = new UpdateCategoryCommand { Name = validName };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-99)]
        public void Should_Have_Error_When_DisplayOrder_Is_Negative(int invalidOrder)
        {
            // Arrange
            var command = new UpdateCategoryCommand { DisplayOrder = invalidOrder };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DisplayOrder)
                  .WithErrorMessage("Display order must be a non-negative integer.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public void Should_Not_Have_Error_When_DisplayOrder_Is_Valid(int validOrder)
        {
            // Arrange
            var command = new UpdateCategoryCommand { DisplayOrder = validOrder };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.DisplayOrder);
        }
    }
}
