
using Bogus;
using E_Shop.Api.Contracts.Category;
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
    public class CreateCategoryControllerTests : BaseIntegrationTest
    {
        private readonly Faker<CreateCategoryRequest> _requestGenerator = new Faker<CreateCategoryRequest>()
            .RuleFor(x => x.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(x => x.DisplayOrder, f => f.Random.Int(1, 100));
        private readonly Faker<Category> _categoryGeneratorForDb = new Faker<Category>()
         .RuleFor(c => c.Name, faker => faker.Commerce.Categories(1)[0])
         .RuleFor(c => c.DisplayOrder, faker => faker.Random.Number(1, 100));

        public CreateCategoryControllerTests(ApplicationApiFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Create_CreateCategory_WhenDataIsValid()
        {
            // Arrange
            var request = _requestGenerator.Generate();

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/categories", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var responseData = await response.Content.ReadFromJsonAsync<CategoryResponse>();

            response.Headers.Location!.ToString().Should().Be($"http://localhost/api/v1/categories/{responseData.Id}");


            responseData.Should().NotBeNull();
            responseData!.Name.Should().Be(request.Name);
            responseData.DisplayOrder.Should().Be(request.DisplayOrder);


            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var categoryInDb = await dbContext.Categories.FindAsync(responseData.Id);

                categoryInDb.Should().NotBeNull();
                categoryInDb.Name.Should().Be(request.Name);
                categoryInDb.DisplayOrder.Should().Be(request.DisplayOrder);
            }
        }

        [Fact]
        public async Task Create_ReturnsValidationError_WhenDataIsInvalid()
        {
            // Arrange
            var category = new CreateCategoryRequest()
            {
                Name = "",
                DisplayOrder = -1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/categories", category);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            problemDetails.Should().NotBeNull();
            problemDetails.Status.Should().Be(400);
            problemDetails.Errors.Should().ContainKey("Name");
            problemDetails.Errors.Should().ContainKey("DisplayOrder");
        }

         [Fact]
        public async Task Create_ShouldInvalidateGetAllCache_WhenNewCategoryIsCreated()
        {
            // Arrange
            var initialCat = _categoryGeneratorForDb.Generate(2);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Categories.AddRange(initialCat);
                await dbContext.SaveChangesAsync();
            }

            await _client.GetAsync("/api/v1/categories");

            var newCategoryRequest = _categoryGeneratorForDb.Generate();

            // Act
            var createResponse = await _client.PostAsJsonAsync("/api/v1/categories", newCategoryRequest);

            var getAllResponse = await _client.GetAsync("/api/v1/categories");

            createResponse.StatusCode.Should().Be(HttpStatusCode.Created); 

            // Assert
            var updatedList = await getAllResponse.Content.ReadFromJsonAsync<List<CategoryResponse>>();

            updatedList.Should().NotBeNull();
            updatedList.Should().HaveCount(3);
            updatedList!.Should().Contain(c => c.Name == newCategoryRequest.Name);
        }

    }
}
