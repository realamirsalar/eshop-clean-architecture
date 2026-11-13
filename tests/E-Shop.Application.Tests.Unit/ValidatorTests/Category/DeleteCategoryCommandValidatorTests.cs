

using E_Shop.Application.Features.Category.Commands;
using E_Shop.Application.Validators.Category;
using FluentValidation.TestHelper;

namespace E_Shop.Application.Tests.Unit.ValidatorTests.Category
{
    public class DeleteCategoryCommandValidatorTests
    {
        private readonly DeleteCategoryCommandValidator _validator;

        public DeleteCategoryCommandValidatorTests()
        {
            _validator = new DeleteCategoryCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Id_Is_Zero_Or_Empty()
        {
            // Arrange 
            var command = new DeleteCategoryCommand(0);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Category id is required.");
        }
        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Should_Have_Error_When_Id_Is_Negative(int invalidId)
        {
            // Arrange
            var command = new DeleteCategoryCommand(invalidId);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Category id must be greater then 0.");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(999)]
        public void Should_Not_Have_Error_When_Id_Is_Valid(int validId)
        {
            // Arrange
            var command = new DeleteCategoryCommand(validId);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
