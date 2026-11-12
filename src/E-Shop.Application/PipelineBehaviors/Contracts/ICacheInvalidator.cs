

namespace E_Shop.Application.PipelineBehaviors.Contracts
{
    public interface ICacheInvalidator
    {
        string[] CacheKeysToInvalidate { get; }
    }
}
