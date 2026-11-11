using E_Shop.Domain;

namespace E_Shop.Application.Common.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Category> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task CreateAsync(Category category , CancellationToken cancellationToken = default);
        void Update(Category category, CancellationToken cancellationToken = default);
        void Delete(Category category, CancellationToken cancellationToken = default);
    }
}
