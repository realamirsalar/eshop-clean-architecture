
namespace E_Shop.Application.PipelineBehaviors.Contracts
{
    public interface ICacheable
    {
        string CacheKey { get; }
        TimeSpan? SlidingExpiration { get; }
        TimeSpan? AbsoluteExpirationRelativeToNow { get; }
        bool BypassCache { get; }
    }
}
