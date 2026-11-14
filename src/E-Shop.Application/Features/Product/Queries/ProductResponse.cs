
using E_Shop.Domain;

namespace E_Shop.Application.Features.Product.Queries
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int CategoryId { get; set; }
        public Domain.Category Category { get; set; }
        public List<ProductImage> ProductImages { get; set; }
    }
}
