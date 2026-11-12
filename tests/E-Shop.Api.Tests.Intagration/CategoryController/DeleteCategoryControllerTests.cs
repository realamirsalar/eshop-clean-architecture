
using Bogus;
using E_Shop.Application.Features.Category.Queries;
using E_Shop.Domain;
using E_Shop.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

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
        [Fact]
        public async Task Delete_ReturnsNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var nonExistentCategoryId = 9999;
            // Act
            var response = await _client.DeleteAsync($"/api/v1/categories/{nonExistentCategoryId}");
            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }
        [Fact]
        public async Task Delete_ReturnsValidationError_WhenIdIsInvalid()
        {
            // Arrange
            var nonExistentCategoryId = -1;
            // Act
            var response = await _client.DeleteAsync($"/api/v1/categories/{nonExistentCategoryId}");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            problemDetails.Should().NotBeNull();
            problemDetails.Status.Should().Be(400);
            problemDetails.Errors.Should().ContainKey("Id");

        }

        [Fact]
        public async Task Delete_ShuoldInvalidateCache_WhenCategoryIsDeleted()
        {
            // Arrange
            var categories = _categoryGenerator.Generate(3);
            var targetCategory = categories[0];

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Categories.AddRange(categories);
                await dbContext.SaveChangesAsync();
            }

            await _client.GetAsync($"/api/v1/categories");
            await _client.GetAsync($"/api/v1/categories/{targetCategory.Id}");

            // Act
            var response = await _client.DeleteAsync($"/api/v1/categories/{targetCategory.Id}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/v1/categories/{targetCategory.Id}");
            getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var getAllResponse = await _client.GetAsync($"/api/v1/categories");
            var list = await getAllResponse.Content.ReadFromJsonAsync<List<CategoryResponse>>();

            list.Should().NotBeNull();
            list.Should().HaveCount(2);
            list.Should().NotContain(c => c.Id == targetCategory.Id);

        }
    }
}
