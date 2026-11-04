using E_Shop.Domain;

namespace E_Shop.Application.Common.Interfaces
{
    public interface ICategoryRepository
    {
        Task Create(Category category , CancellationToken cancellationToken = default);
    }
}
