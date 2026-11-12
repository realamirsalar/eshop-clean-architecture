

using AutoMapper;
using E_Shop.Application.Common.Errors;
using E_Shop.Application.Common.Interfaces;
using E_Shop.Application.Common.Utility;
using E_Shop.Application.Features.Category.Queries;
using E_Shop.Application.PipelineBehaviors.Contracts;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace E_Shop.Application.Features.Category.Commands
{
    public class UpdateCategoryCommand : IRequest<Result<CategoryResponse>>, IValidatable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
    }
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _distributedCache;

        public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper, IDistributedCache distributedCache)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _distributedCache = distributedCache;
        }

        public async Task<Result<CategoryResponse>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                return Result.Fail(new NotFoundError($"Category with Id {request.Id} not found"));
            }
            _mapper.Map(request, category);
            _categoryRepository.Update(category);
            var changes = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (changes == 0)
            {
                return Result.Fail(new Error("Failed to update product."));
            }
            try
            {
                var removeListTask = _distributedCache.RemoveAsync($"{SD.Category}_List", cancellationToken);
                var removeSingleTask = _distributedCache.RemoveAsync($"{SD.Category}_{request.Id}", cancellationToken);
                await Task.WhenAll(removeListTask,removeSingleTask);
            }
            catch (Exception)
            {

            }
            var response = _mapper.Map<CategoryResponse>(category);
            return Result.Ok(response);
        }
    }
}
