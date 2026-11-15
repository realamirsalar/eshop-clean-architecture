

using E_Shop.Application.Common.Interfaces;
using E_Shop.Domain;
using E_Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
        public void Delete(Category category, CancellationToken cancellationToken = default)
        {
            _db.Categories.Remove(category);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (category == null)
            {
                return false;
            }
            return true;
        }

        public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Categories.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Category> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public void Update(Category category, CancellationToken cancellationToken = default)
        {
            _db.Categories.Update(category);
        }
    }
}
