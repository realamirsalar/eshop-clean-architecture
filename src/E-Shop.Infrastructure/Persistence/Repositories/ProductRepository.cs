
using E_Shop.Application.Common.Interfaces;
using E_Shop.Domain;
using E_Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_Shop.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _db.AddAsync(product, cancellationToken);
        }

        public async Task ExecuteDeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            await _db.Products.Where(p => p.Id == id).ExecuteDeleteAsync(cancellationToken);
        }

        public Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _db.Products.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Product> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public void Update(Product product, CancellationToken cancellationToken = default)
        {
            _db.Products.Update(product);
        }
    }
}
