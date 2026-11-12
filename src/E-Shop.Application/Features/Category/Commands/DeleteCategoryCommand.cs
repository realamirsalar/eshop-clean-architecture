using E_Shop.Application.Common.Errors;
using E_Shop.Application.Common.Interfaces;
using E_Shop.Application.Common.Utility;
using E_Shop.Application.PipelineBehaviors.Contracts;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;


namespace E_Shop.Application.Features.Category.Commands
{
    public record DeleteCategoryCommand(int Id) : IRequest<Result>, IValidatable;

    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _distributedCache;
        public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IDistributedCache distributedCache)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _distributedCache = distributedCache;
        }

        public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                return Result.Fail(new NotFoundError($"Category with Id {request.Id} not found"));
            }
            _categoryRepository.Delete(category);
            var changes = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (changes == 0)
            {
                return Result.Fail(new Error("Failed to delete category"));
            }
            try
            {
                var removeListTask = _distributedCache.RemoveAsync($"{SD.Category}_List", cancellationToken);
                var removeSingleTask = _distributedCache.RemoveAsync($"{SD.Category}_{request.Id}", cancellationToken);
                await Task.WhenAll(removeListTask, removeSingleTask);
            }
            catch (Exception)
            {

            }
            return Result.Ok();
        }
    }
}
