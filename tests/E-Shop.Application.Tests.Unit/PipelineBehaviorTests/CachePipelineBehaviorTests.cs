using E_Shop.Application.Common.CacheOptions;
using E_Shop.Application.PipelineBehaviors;
using E_Shop.Application.PipelineBehaviors.Contracts;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace E_Shop.Application.Tests.Unit.PipelineBehaviorTests
{

    public class CachePipelineBehaviorTests
    {
        // ۱. ساخت یک مدل واقعی که قرار است کش شود
        public class SampleDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }
        // ۲. شبیه‌سازی اینترفیس ICacheable
        public class DummyCacheRequest : IRequest<Result<SampleDto>>, ICacheable
        {
            public string CacheKey => "sample-key";
            public bool BypassCache { get; set; }
            public TimeSpan? SlidingExpiration => null;
            public TimeSpan? AbsoluteExpirationRelativeToNow => null;
        }
        private readonly IDistributedCache _cacheMock;
        private readonly ILogger<CachePipelineBehavior<DummyCacheRequest, Result<SampleDto>>> _loggerMock;
        private readonly IOptions<CacheSettings> _cacheSettings;
        private readonly CachePipelineBehavior<DummyCacheRequest, Result<SampleDto>> _behavior;

        public CachePipelineBehaviorTests()
        {
            _cacheMock = Substitute.For<IDistributedCache>();
            _loggerMock = Substitute.For<ILogger<CachePipelineBehavior<DummyCacheRequest, Result<SampleDto>>>>();

            _cacheSettings = Options.Create(new CacheSettings
            {
                ByPassCache = false,
                SlidingExpirationMinutes = 10,
                AbsoluteExpirationHours = 1
            });

            _behavior = new CachePipelineBehavior<DummyCacheRequest, Result<SampleDto>>(
                _cacheMock, _loggerMock, _cacheSettings);
        }

        // ====================================================================
        // تست اول: وقتی ByPassCache روشن است، باید ردیس را دور بزند
        // ====================================================================
        [Fact]
        public async Task Handle_ShouldBypassCache_WhenBypassCacheIsTrue()
        {
            // Arrange
            var request = new DummyCacheRequest { BypassCache = true };
            var next = Substitute.For<RequestHandlerDelegate<Result<SampleDto>>>();
            next().Returns(Result.Ok(new SampleDto { Id = 1, Title = "From DB" }));

            // Act
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.Value.Title.Should().Be("From DB");


            await _cacheMock.DidNotReceiveWithAnyArgs().GetAsync(default, default);
            await next.Received(1)();
        }
        // ====================================================================
        // تست دوم: Cache Hit - دیتا در ردیس هست، پس دیتابیس (next) نباید اجرا شود
        // ====================================================================
        [Fact]
        public async Task Handle_ShouldReturnCachedData_WhenDataExistsInRedis()
        {
            // Arrange
            var request = new DummyCacheRequest();
            var cachedData = new SampleDto { Id = 1, Title = "From Cache" };
            var cachedBytes = JsonSerializer.SerializeToUtf8Bytes(cachedData);

            // شبیه‌سازی دیتای موجود در ردیس
            _cacheMock.GetAsync(request.CacheKey, Arg.Any<CancellationToken>())
                      .Returns(cachedBytes);

            var next = Substitute.For<RequestHandlerDelegate<Result<SampleDto>>>();

            // Act
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Title.Should().Be("From Cache"); 

            await next.DidNotReceive()();
        }
        // ====================================================================
        // تست سوم: Cache Miss (موفقیت‌آمیز) - دیتا نیست، از دیتابیس می‌گیرد و در کش می‌نویسد
        // ====================================================================
        [Fact]
        public async Task Handle_ShouldCallNextAndSetCache_WhenDataNotInRedis()
        {
            // Arrange
            var request = new DummyCacheRequest();
            var dbData = new SampleDto { Id = 1, Title = "From DB" };

            _cacheMock.GetAsync(request.CacheKey, Arg.Any<CancellationToken>())
                      .Returns((byte[])null); 

            var next = Substitute.For<RequestHandlerDelegate<Result<SampleDto>>>();
            next().Returns(Result.Ok(dbData));

            // Act
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Title.Should().Be("From DB");

            await next.Received(1)();


            await _cacheMock.Received(1).SetAsync(
                request.CacheKey,
                Arg.Any<byte[]>(),
                Arg.Any<DistributedCacheEntryOptions>(),
                Arg.Any<CancellationToken>());
        }

        // ====================================================================
        // تست چهارم: جلوگیری از کش شدن نتیجه Failed (مثل 404)
        // ====================================================================
        [Fact]
        public async Task Handle_ShouldNotSetCache_WhenResultIsFailed()
        {
            // Arrange
            var request = new DummyCacheRequest();

            _cacheMock.GetAsync(request.CacheKey, Arg.Any<CancellationToken>())
                      .Returns((byte[])null);

            var next = Substitute.For<RequestHandlerDelegate<Result<SampleDto>>>();

            next().Returns(Result.Fail<SampleDto>("رکورد یافت نشد"));

            // Act
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Be("رکورد یافت نشد");

            await next.Received(1)();

            // 🚨 اثبات اینکه ارورها هرگز در ردیس کش نمی‌شوند!
            await _cacheMock.DidNotReceiveWithAnyArgs().SetAsync(default, default!, default!, default);
        }

        // ====================================================================
        // تست پنجم (تاب‌آوری): اگر سرور ردیس خاموش بود، API نباید کرش کند!
        // ====================================================================
        [Fact]
        public async Task Handle_ShouldFallbackToNext_WhenRedisThrowsException()
        {
            // Arrange
            var request = new DummyCacheRequest();
            var dbData = new SampleDto { Id = 1, Title = "From DB After Redis Failed" };

            // شبیه‌سازی قطع بودن ردیس (Throw کردن Exception)
            _cacheMock.GetAsync(request.CacheKey, Arg.Any<CancellationToken>())
                      .Throws(new Exception("Redis connection failed!"));

            var next = Substitute.For<RequestHandlerDelegate<Result<SampleDto>>>();
            next().Returns(Result.Ok(dbData));

            // Act 
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.Value.Title.Should().Be("From DB After Redis Failed");
            await next.Received(1)();
        }
    }
}
