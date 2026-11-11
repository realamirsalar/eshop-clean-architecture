
using Bogus;
using E_Shop.Api.Contracts.Category;
using E_Shop.Application.Features.Category.Queries;
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
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            //response.StatusCode.Should().Be(HttpStatusCode.Created);
            //response.Headers.Location.Should().NotBeNull();
            //response.Headers.Location!.ToString().Should().Be($"http://localhost/api/v1/categories/{createdCategory.Id}");

            var responseData = await response.Content.ReadFromJsonAsync<CategoryResponse>();

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
    }
}
