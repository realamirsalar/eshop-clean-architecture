
using E_Shop.Application.PipelineBehaviors;
using FluentAssertions;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;

namespace E_Shop.Application.Tests.Unit.PipelineBehaviorTests
{

    public class ValidationPipelineBehaviorTests
    {
        public class DummyCommand : IRequest<Result> { }
        public class DummyQuery : IRequest<Result<string>> { }
        public class UnsupportedCommand : IRequest<string> { } // برای تست خطای Exception

        private readonly DummyCommand _dummyCommand = new DummyCommand();
        private readonly IValidator<DummyCommand> _dummyCommandValidator = Substitute.For<IValidator<DummyCommand>>();
        [Fact]
        public async Task Handle_ShouldCallNext_WhenValidationIsSuccessful()
        {
            // Arrange

            // شبیه‌سازی ولیدیشن موفق (بدون ارور)

            _dummyCommandValidator.ValidateAsync(Arg.Any<ValidationContext<DummyCommand>>(), Arg.Any<CancellationToken>())
                .Returns(new ValidationResult());

            var behavior = new ValidationPipelineBehaviors<DummyCommand, Result>(new[] { _dummyCommandValidator });

            var next = Substitute.For<RequestHandlerDelegate<Result>>();
            next().Returns(Result.Ok());

            // Act
            var result = await behavior.Handle(_dummyCommand, next, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await next.Received(1)();
        }

        [Fact]
        public async Task Handle_ShouldReturnsFaliedResult_WhenValidationFails()
        {
            //Arrange
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Title", "عنوان الزامی است")
            };

            _dummyCommandValidator.ValidateAsync(Arg.Any<ValidationContext<DummyCommand>>(), Arg.Any<CancellationToken>())
              .Returns(new ValidationResult(failures));


            var behavior = new ValidationPipelineBehaviors<DummyCommand, Result>(new[] { _dummyCommandValidator });
            var next = Substitute.For<RequestHandlerDelegate<Result>>();

            // Act
            var result = await behavior.Handle(_dummyCommand, next, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Message.Should().Be("عنوان الزامی است");

            result.Errors[0].Metadata.Should().ContainKey("PropertyName")
                                 .WhoseValue.Should().Be("Title");

            await next.DidNotReceive()();

        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperationException_WhenReturnTypeIsUnsupported()
        {
            // Arrange (استفاده از کلاسی که خروجی string دارد، نه Result)
            var request = new UnsupportedCommand();
            var validator = Substitute.For<IValidator<UnsupportedCommand>>();
            var failures = new List<ValidationFailure> { new ValidationFailure("Prop", "Err") };

            validator.ValidateAsync(Arg.Any<ValidationContext<UnsupportedCommand>>(), Arg.Any<CancellationToken>())
                     .Returns(new ValidationResult(failures));

            var behavior = new ValidationPipelineBehaviors<UnsupportedCommand, string>(new[] { validator });
            var next = Substitute.For<RequestHandlerDelegate<string>>();

            // Act
            Func<Task> action = async () => await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            // چون خروجی از نوع Result نیست و ارور داریم، باید Exception پرتاب شود
            await action.Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*در پایپ‌لاین ولیدیشن پشتیبانی نمی‌شود*");

            await next.DidNotReceive()();
        }
    }
}
