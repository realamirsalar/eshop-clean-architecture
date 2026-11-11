
using Bogus;
using E_Shop.Application.Features.Category.Queries;
using E_Shop.Domain;
using E_Shop.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace E_Shop.Api.Tests.Intagration.CategoryController
{
    public class GetAllCategoriesControllerTests : BaseIntegrationTest
    {
        private readonly Faker<Category> _faker = new Faker<Category>()
            .RuleFor(x => x.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(x => x.DisplayOrder, f => f.Random.Int(1, 100));
        public GetAllCategoriesControllerTests(ApplicationApiFactory factory) : base(factory)
        {
            
        }
        [Fact]
        public async Task GetAll_ReturnsCategoriesList_WhenCategoryExists()
        {
            // Arrange 
            var categories = _faker.Generate(3);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Categories.AddRange(categories);
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync("api/v1/categories");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var fetchedCategories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();

            fetchedCategories.Should().NotBeNull();
            fetchedCategories.Should().HaveCount(3);
            fetchedCategories.Should().BeEquivalentTo(categories , option => option.ExcludingMissingMembers());
        }
        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoCategoryExists()
        {
            // Arrange

            // Act
            var response = await _client.GetAsync("api/v1/categories");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var fetchedCategories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
            fetchedCategories.Should().BeEmpty();
        }
    }
}
