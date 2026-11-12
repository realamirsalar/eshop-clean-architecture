using E_Shop.Application.PipelineBehaviors.Contracts;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace E_Shop.Application.PipelineBehaviors
{
    public class CacheInvalidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheInvalidator
    {
        private readonly IDistributedCache _cache;

        public CacheInvalidationPipelineBehavior(
            IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var response = await next();

            // اگر عملیات در هندلر شکست خورده (مثلاً اعتبارسنجی یا خطای دیتابیس)، کش را پاک نمی‌کنیم
            bool isFluentResult = typeof(IResultBase).IsAssignableFrom(typeof(TResponse));
            if (isFluentResult && ((IResultBase)response).IsFailed)
            {
                return response;
            }

            // عملیات موفق بوده؛ حالا کلیدها را از کش پاک می‌کنیم
            try
            {
                var tasks = request.CacheKeysToInvalidate.Select(key =>
                {
                    return _cache.RemoveAsync(key, cancellationToken);
                });

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Redis Cache Invalidation Error for Keys: {Keys}", string.Join(", ", request.CacheKeysToInvalidate));
            }

            return response;
        }
    }
}
