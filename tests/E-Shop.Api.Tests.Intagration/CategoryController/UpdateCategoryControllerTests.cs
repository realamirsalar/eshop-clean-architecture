
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
    public class UpdateCategoryControllerTests : BaseIntegrationTest
    {
        private readonly Faker<Category> _categoryGenerator = new Faker<Category>()
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(c => c.DisplayOrder, f => f.Random.Number(1, 50));
        private readonly Faker<UpdateCategoryRequest> _requestGenerator = new Faker<UpdateCategoryRequest>()
            .RuleFor(x => x.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(x => x.DisplayOrder, f => f.Random.Int(1, 100));
        public UpdateCategoryControllerTests(ApplicationApiFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Update_ReturnsUpdatedCategory_WhenCategoryUpdated()
        {
            // Arrange
            var category = _categoryGenerator.Generate();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Categories.Add(category);
                await dbContext.SaveChangesAsync();
            }

            var updateRequest = _requestGenerator.Generate();
            updateRequest.Id = category.Id;

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/categories/{category.Id}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var assertScope = _factory.Services.CreateScope())
            {
                var assertDbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var categoryInDb = await assertDbContext.Categories.FindAsync(category.Id);

                categoryInDb.Should().NotBeNull();

                categoryInDb!.Name.Should().Be(updateRequest.Name);
                categoryInDb.DisplayOrder.Should().Be(updateRequest.DisplayOrder);

                categoryInDb.Name.Should().NotBe(category.Name);
            }
        }
        [Fact]
        public async Task Update_ReturnsNotFound_WhenCategoryIsNotExists()
        {
            // Arrange
            var updateRequest = _requestGenerator.Generate();
            updateRequest.Id = 9999; // فرض می‌کنیم این آی‌دی وجود ندارد
            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/categories/{updateRequest.Id}", updateRequest);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_ReturnsValidationError_WhenDataIsInvalid()
        {
            // Arrange
            var category = _categoryGenerator.Generate();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Categories.Add(category);
                await dbContext.SaveChangesAsync();
            }

            var updateRequest = new UpdateCategoryRequest
            {
                Id = category.Id,
                Name = "", // نام خالی برای تست اعتبارسنجی
                DisplayOrder = -1 // مقدار منفی برای تست اعتبارسنجی
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/categories/{updateRequest.Id}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            problemDetails.Should().NotBeNull();
            problemDetails!.Status.Should().Be(400);
            problemDetails.Errors.Should().ContainKey("Name");
            problemDetails.Errors.Should().ContainKey("DisplayOrder");

            using (var assertScope = _factory.Services.CreateScope())
            {
                var assertDbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var categoryInDb = await assertDbContext.Categories.FindAsync(category.Id);


                categoryInDb.Should().NotBeNull();

                // بررسی می‌کنیم که دیتای دیتابیس، به دیتای جدیدی که باگوس در updateRequest ساخته بود تغییر کرده باشد
                categoryInDb!.Name.Should().Be(category.Name);
                categoryInDb.DisplayOrder.Should().Be(category.DisplayOrder);

                // بررسی می‌کنیم که تصادفاً با اسم قبلی برابر نمانده باشد (اختیاری ولی برای اطمینان عالی است)
                categoryInDb.Name.Should().NotBe(updateRequest.Name);
            }

        }

        [Fact]
        public async Task Update_ShouldInvalidateCache_WhenCategoryIsUpdated()
        {
            // Arrange
            var category = _categoryGenerator.Generate();
            var originalName = category.Name;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Categories.Add(category);
                await dbContext.SaveChangesAsync();
            }

            await _client.GetAsync($"/api/v1/categories");
            await _client.GetAsync($"/api/v1/categories/{category.Id}");

            var updatedName = "Updated Name";
            var updateRequest = new UpdateCategoryRequest
            {
                Id = category.Id,
                Name = updatedName,
                DisplayOrder = category.DisplayOrder
            };

            // Act 
            var response = await _client.PutAsJsonAsync($"/api/v1/categories/{category.Id}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var getResponse = await _client.GetAsync($"/api/v1/categories/{category.Id}");
            var fetchedCategory = await getResponse.Content.ReadFromJsonAsync<CategoryResponse>();

            fetchedCategory.Should().NotBeNull();
            fetchedCategory!.Name.Should().Be(updatedName);

            var getAllResponse = await _client.GetAsync($"/api/v1/categories");
            var list = await getAllResponse.Content.ReadFromJsonAsync<List<CategoryResponse>>();

            list.Should().Contain(c => c.Name == updatedName);
            list.Should().NotContain(c => c.Name == originalName);
        }

    }
}
