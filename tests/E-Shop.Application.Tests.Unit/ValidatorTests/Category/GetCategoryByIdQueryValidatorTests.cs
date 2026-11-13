using E_Shop.Application.Features.Category.Queries;
using E_Shop.Application.Validators.Category;
using FluentValidation.TestHelper;


namespace E_Shop.Application.Tests.Unit.ValidatorTests.Category
{
    public class GetCategoryByIdQueryValidatorTests
    {
        private readonly GetCategoryByIdQueryValidator _validator;

        public GetCategoryByIdQueryValidatorTests()
        {
            _validator = new GetCategoryByIdQueryValidator();
        }

        // ====================================================================
        // تست بررسی NotEmpty (مقدار صفر)
        // ====================================================================
        [Fact]
        public void Should_Have_Error_When_Id_Is_Zero_Or_Empty()
        {
            // Arrange
            var query = new GetCategoryByIdQuery(0);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Category id is required.");
        }

        // ====================================================================
        // تست بررسی GreaterThan (اعداد منفی)
        // ====================================================================
        [Theory]
        [InlineData(-1)]
        [InlineData(-50)]
        public void Should_Have_Error_When_Id_Is_Negative(int invalidId)
        {
            // Arrange
            var query = new GetCategoryByIdQuery(invalidId);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Category id must be greater then 0.");
        }

        // ====================================================================
        // تست سناریوی موفق (Happy Path)
        // ====================================================================
        [Theory]
        [InlineData(1)]
        [InlineData(25)]
        [InlineData(1000)]
        public void Should_Not_Have_Error_When_Id_Is_Valid(int validId)
        {
            // Arrange
            var query = new GetCategoryByIdQuery(validId);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
