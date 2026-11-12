
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
    public class GetByIdCategoryControllerTests : BaseIntegrationTest
    {
        private readonly Faker<Category> _requestGenerator = new Faker<Category>()
            .RuleFor(x => x.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(x => x.DisplayOrder, f => f.Random.Int(1, 100));
        public GetByIdCategoryControllerTests(ApplicationApiFactory factory) : base(factory)
        {
            
        }

        [Fact]
        public async Task GetById_ReturnsExpectedCategory_WhenCategoryIsExsist()
        {
            // Arrange
            var category = _requestGenerator.Generate();
            
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Categories.Add(category);
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync($"/api/v1/categories/{category.Id}");

            // Arrange
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var fetchedCategory = await response.Content.ReadFromJsonAsync<CategoryResponse>();

            fetchedCategory.Should().NotBeNull();
            fetchedCategory.Name.Should().Be(category.Name);
            fetchedCategory.DisplayOrder.Should().Be(category.DisplayOrder);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenCategoryIsNotExists()
        {
            // Arrange
            var nonExistentCategoryId = 9999;
            // Act
            var response = await _client.GetAsync($"/api/v1/categories/{nonExistentCategoryId}");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_ReturnsValidationError_WhenIdIsInvalid()
        {
            // Arrange
            var nonExistentCategoryId = -1;
            // Act
            var response = await _client.GetAsync($"/api/v1/categories/{nonExistentCategoryId}");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            problemDetails.Should().NotBeNull();
            problemDetails.Status.Should().Be(400);
            problemDetails.Errors.Should().ContainKey("Id");

        }
    }
}
