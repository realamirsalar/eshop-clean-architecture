

using Bogus;
using E_Shop.Application.Features.Product.Queries;
using E_Shop.Domain;
using E_Shop.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace E_Shop.Api.Tests.Intagration.ProductController
{
    public class GetProductByIdControllerTests(ApplicationApiFactory apiFactory) : BaseIntegrationTest(apiFactory)
    {
        private readonly Faker<Product> _productGenerator = new Faker<Product>()
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.Description, f => f.Lorem.Paragraph())
            .RuleFor(x => x.Price, f => (int)(f.Finance.Amount(10000, 100000)))
            .RuleFor(x => x.Quantity, f => f.Random.Int(1, 100));
        private readonly Faker<Category> _categoryGeneratorForDb = new Faker<Category>()
         .RuleFor(c => c.Name, faker => faker.Commerce.Categories(1)[0])
         .RuleFor(c => c.DisplayOrder, faker => faker.Random.Number(1, 100));
        [Fact]
        public async Task GetProductById_ReturnsProduct_WhenProductExists()
        {
            // Arrange
            var category = _categoryGeneratorForDb.Generate();
            var product = _productGenerator.Generate();
            product.Category = category;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync($"/api/v1/products/{product.Id}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var productResponse = await response.Content.ReadFromJsonAsync<ProductResponse>();
            productResponse.Should().NotBeNull();
            productResponse.Should().BeEquivalentTo(new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Quantity,
                product.CategoryId
            });
        }
        [Fact]
        public async Task GetProductById_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            var nonExistentProductId = 9999; // Assuming this product ID does not exist
            // Act
            var response = await _client.GetAsync($"/api/v1/products/{nonExistentProductId}");
            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }
    }
}
