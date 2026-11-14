

using E_Shop.Domain;

namespace E_Shop.Application.Common.Interfaces
{
    public interface IProductRepository
    {

        Task AddAsync(Product product, CancellationToken cancellationToken = default);
        Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Product> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        void Update(Product product, CancellationToken cancellationToken = default);
        Task ExecuteDeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
