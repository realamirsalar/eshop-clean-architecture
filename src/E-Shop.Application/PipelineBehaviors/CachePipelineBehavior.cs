

using E_Shop.Application.Common.CacheOptions;
using E_Shop.Application.PipelineBehaviors.Contracts;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json;

namespace E_Shop.Application.PipelineBehaviors
{
    public class CachePipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IRequest<TResponse>, ICacheable
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CachePipelineBehavior<TRequest, TResponse>> _logger;
        private readonly CacheSettings _cacheSettings;
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public CachePipelineBehavior(
            IDistributedCache cache,
            ILogger<CachePipelineBehavior<TRequest, TResponse>> logger,
            IOptions<CacheSettings> cacheSettings)
        {
            _cache = cache;
            _logger = logger;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request.BypassCache || _cacheSettings.ByPassCache)
            {
                return await next();
            }

            bool isFluentResult = typeof(IResultBase).IsAssignableFrom(typeof(TResponse));
            bool isGenericResult = typeof(TResponse).IsGenericType &&
                                   typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>);

            Type valueType = isGenericResult ? typeof(TResponse).GetGenericArguments()[0] : typeof(TResponse);

            try
            {
                var cachedBytes = await _cache.GetAsync(request.CacheKey, cancellationToken);


                if (cachedBytes is not null && cachedBytes.Length > 0)
                {
                    _logger.LogInformation("Fetched from Cache -> Key: {CacheKey}", request.CacheKey);
                    var cachedData = JsonSerializer.Deserialize(cachedBytes, valueType, SerializerOptions);

                    if (isGenericResult)
                    {
                        var okMethod = typeof(Result)
                            .GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .First(m => m.Name == "Ok" && m.IsGenericMethod && m.GetParameters().Length == 1)
                            .MakeGenericMethod(valueType);

                        return (TResponse)okMethod.Invoke(null, new[] { cachedData })!;
                    }

                    return (TResponse)cachedData!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis Cache Read Error for Key: {CacheKey}. Falling back to database.", request.CacheKey);
            }
            var response = await next();

            if (response is not null)
            {
                if (isFluentResult && ((IResultBase)response).IsFailed)
                {
                    return response;
                }

                try
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = request.SlidingExpiration ?? TimeSpan.FromMinutes(_cacheSettings.SlidingExpirationMinutes),
                        AbsoluteExpirationRelativeToNow = request.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromHours(_cacheSettings.AbsoluteExpirationHours)
                    };

                    object? dataToCache = response;
                    if (isGenericResult)
                    {
                        var valueProperty = typeof(TResponse).GetProperty("Value");
                        dataToCache = valueProperty?.GetValue(response);
                    }

                    var dataBytes = JsonSerializer.SerializeToUtf8Bytes(dataToCache, SerializerOptions);
                    await _cache.SetAsync(request.CacheKey, dataBytes, options, cancellationToken);

                    _logger.LogInformation("Added to Cache -> Key: {CacheKey}", request.CacheKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Redis Cache Write Error for Key: {CacheKey}", request.CacheKey);
                }
            }

            return response;
        }
    }

}
