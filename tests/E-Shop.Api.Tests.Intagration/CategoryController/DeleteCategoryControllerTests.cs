
using Bogus;
using E_Shop.Domain;
using E_Shop.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace E_Shop.Api.Tests.Intagration.CategoryController
{
    public class DeleteCategoryControllerTests(ApplicationApiFactory factory) :BaseIntegrationTest(factory)
    {
        private readonly Faker<Category> _categoryGenerator = new Faker<Category>()
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(c => c.DisplayOrder, f => f.Random.Number(1, 50));

        //public DeleteCategoryControllerTests(ApplicationApiFactory factory) : base(factory)
        //{
        //}

        [Fact]
        public async Task Delete_DeleteCategory_WhenCategoryIsExsist()
        {
            // Arrange
            var category = _categoryGenerator.Generate();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Categories.Add(category);
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.DeleteAsync($"/api/v1/categories/{category.Id}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

            using (var assertScope = _factory.Services.CreateScope())
            {
                var assertDbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var categoryInDb = await assertDbContext.Categories.FindAsync(category.Id);
                categoryInDb.Should().BeNull();
            }
        }
    }
}
