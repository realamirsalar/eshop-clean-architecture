
using E_Shop.Application.PipelineBehaviors;
using E_Shop.Application.PipelineBehaviors.Contracts;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace E_Shop.Application.Tests.Unit.PipelineBehaviorTests
{
    public class CacheInvalidationPipelineBehaviorTests
    {
        public class SampleDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
        }

        // شبیه‌سازی کامندی که از ICacheInvalidator ارث‌بری کرده است
        public class DummyInvalidatorCommand : IRequest<Result<SampleDto>>, ICacheInvalidator
        {
            public string[] CacheKeysToInvalidate { get; set; } = new[] { "Category_List", "Category_1" };
        }

        private readonly IDistributedCache _cacheMock;
        private readonly CacheInvalidationPipelineBehavior<DummyInvalidatorCommand, Result<SampleDto>> _behavior;

        public CacheInvalidationPipelineBehaviorTests()
        {
            _cacheMock = Substitute.For<IDistributedCache>();
            _behavior = new CacheInvalidationPipelineBehavior<DummyInvalidatorCommand, Result<SampleDto>>(_cacheMock);
        }

        // ====================================================================
        // تست اول (Happy Path): وقتی هندلر موفق است، تمام کلیدها باید از ردیس پاک شوند
        // ====================================================================
        [Fact]
        public async Task Handle_ShouldRemoveAllSpecifiedCacheKeys_WhenHandlerResultIsSuccess()
        {
            // Arrange
            var request = new DummyInvalidatorCommand
            {
                CacheKeysToInvalidate = new[] { "Category_List", "Category_1" }
            };

            var expectedDto = new SampleDto { Id = 1, Title = "Created Category" };
            var next = Substitute.For<RequestHandlerDelegate<Result<SampleDto>>>();
            next().Returns(Result.Ok(expectedDto));

            // Act
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedDto);

            await next.Received(1)();

            // بررسی اینکه متد RemoveAsync دقیقاً برای تک‌تک کلیدها صدا زده شده است
            await _cacheMock.Received(1).RemoveAsync("Category_List", Arg.Any<CancellationToken>());
            await _cacheMock.Received(1).RemoveAsync("Category_1", Arg.Any<CancellationToken>());
        }

        // ====================================================================
        // تست دوم (Failure Path): وقتی هندلر Result.Fail برمی‌گرداند، کش نباید پاک شود
        // ====================================================================
        [Fact]
        public async Task Handle_ShouldNotInvalidateCache_WhenHandlerResultIsFailed()
        {
            // Arrange
            var request = new DummyInvalidatorCommand();
            var next = Substitute.For<RequestHandlerDelegate<Result<SampleDto>>>();
            next().Returns(Result.Fail<SampleDto>("خطا در ذخیره‌سازی اطلاعات"));

            // Act
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Be("خطا در ذخیره‌سازی اطلاعات");

            await next.Received(1)();

            // 🚨 اثبات اینکه در صورت شکست عملیات دیتابیس، به کش دست زده نمی‌شود
            await _cacheMock.DidNotReceiveWithAnyArgs().RemoveAsync(default!, default);
        }

        // ====================================================================
        // تست سوم (تاب‌آوری / Resiliency): اگر ردیس قطع بود، عملیات موفق دیتابیس نباید کرش کند
        // ====================================================================
        [Fact]
        public async Task Handle_ShouldSwallowExceptionAndReturnSuccess_WhenRedisThrowsException()
        {
            // Arrange
            var request = new DummyInvalidatorCommand();
            var expectedDto = new SampleDto { Id = 1, Title = "Updated Successfully" };

            var next = Substitute.For<RequestHandlerDelegate<Result<SampleDto>>>();
            next().Returns(Result.Ok(expectedDto));

            // شبیه‌سازی قطع بودن ارتباط با سرور ردیس هنگام پاک کردن کش
            _cacheMock.RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                      .Throws(new Exception("Redis server is down!"));

            // Act
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            // با اینکه ردیس خطا داده، چون دیتا در دیتابیس ذخیره شده، کاربر باید پاسخ موفق بگیرد
            result.IsSuccess.Should().BeTrue();
            result.Value.Title.Should().Be("Updated Successfully");

            await next.Received(1)();
        }

        // ====================================================================
        // تست چهارم (Edge Case): اگر آرایه کلیدها خالی بود، نباید خطایی رخ دهد یا ردیس صدا زده شود
        // ====================================================================
        [Fact]
        public async Task Handle_ShouldNotCallRemoveAsync_WhenCacheKeysArrayIsEmpty()
        {
            // Arrange
            var request = new DummyInvalidatorCommand
            {
                CacheKeysToInvalidate = Array.Empty<string>()
            };

            var next = Substitute.For<RequestHandlerDelegate<Result<SampleDto>>>();
            next().Returns(Result.Ok(new SampleDto { Id = 1, Title = "Test" }));

            // Act
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await next.Received(1)();
            await _cacheMock.DidNotReceiveWithAnyArgs().RemoveAsync(default!, default);
        }
    }
}
