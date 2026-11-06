using E_Shop.Application.PipelineBehaviors.Contracts;
using FluentResults;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace E_Shop.Application.PipelineBehaviors
{

    public class ValidationPipelineBehaviors<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>, IValidatable
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        private static readonly Lazy<MethodInfo?> _genericFailMethod = new(() =>
        {
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse).GetGenericArguments()[0];
                return typeof(Result).GetMethods()
                    .First(m => m.Name == nameof(Result.Fail) &&
                                m.IsGenericMethod &&
                                m.GetParameters().Length == 1 &&
                                m.GetParameters()[0].ParameterType == typeof(IEnumerable<IError>))
                    .MakeGenericMethod(resultType);
            }
            return null;
        });


        public ValidationPipelineBehaviors(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count > 0)
            {
                var fluentErrors = failures.Select(f =>
                    new Error(f.ErrorMessage)
                        .WithMetadata("PropertyName", f.PropertyName)
                ).Cast<IError>().ToList();

                if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.Fail(fluentErrors);
                }

                if (_genericFailMethod.Value != null)
                {
                    var result = _genericFailMethod.Value.Invoke(null, new object[] { fluentErrors });
                    return (TResponse)result!;
                }

                throw new InvalidOperationException($"نوع {typeof(TResponse).Name} در پایپ‌لاین ولیدیشن پشتیبانی نمی‌شود.");
            }

            return await next();
        }
    }
}
