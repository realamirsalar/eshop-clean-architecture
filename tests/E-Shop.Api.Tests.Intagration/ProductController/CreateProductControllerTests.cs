

using Bogus;
using E_Shop.Api.Contracts.Product;
using E_Shop.Application.Features.Product.Queries;
using E_Shop.Domain;
using E_Shop.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace E_Shop.Api.Tests.Intagration.ProductController
{
    public class CreateProductControllerTests(ApplicationApiFactory factory) : BaseIntegrationTest(factory)
    {
        private readonly Faker<CreateProductRequest> _productGenerator = new Faker<CreateProductRequest>()
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.Description, f => f.Lorem.Paragraph())
            .RuleFor(x => x.Price, f => (int)(f.Finance.Amount(10000, 100000)))
            .RuleFor(x => x.Quantity, f => f.Random.Int(1, 100));
        private readonly Faker<Category> _categoryGeneratorForDb = new Faker<Category>()
         .RuleFor(c => c.Name, faker => faker.Commerce.Categories(1)[0])
         .RuleFor(c => c.DisplayOrder, faker => faker.Random.Number(1, 100));

        [Fact]
        public async Task Create_ReturnsCreatedProduct_WhenDataIsValid()
        {
            // Arrange
            var category = _categoryGeneratorForDb.Generate();
            using var scope = _factory.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            var product = _productGenerator.Generate();
            product.CategoryId = category.Id;

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/products", product);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var createdProduct = await response.Content.ReadFromJsonAsync<ProductResponse>();
            createdProduct.Should().NotBeNull();
            createdProduct.Should().BeEquivalentTo(product, options => options
                .ExcludingMissingMembers()
            );

            var productInDb = await dbContext.Products.FindAsync(createdProduct.Id);

            productInDb.Should().NotBeNull();
            productInDb.Should().BeEquivalentTo(product, options => options
                .ExcludingMissingMembers()
            );
        }

    }
}
