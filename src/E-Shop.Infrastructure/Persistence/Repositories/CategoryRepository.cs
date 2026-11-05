

using E_Shop.Application.Common.Interfaces;
using E_Shop.Domain;
using E_Shop.Infrastructure.Data;

namespace E_Shop.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _db.Categories.AddAsync(category, cancellationToken);
        }
    }
}
