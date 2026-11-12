using AutoMapper;
using E_Shop.Application.Common.Interfaces;
using E_Shop.Application.Common.Utility;
using E_Shop.Application.Features.Category.Queries;
using E_Shop.Application.PipelineBehaviors.Contracts;

using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace E_Shop.Application.Features.Category.Commands
{
    public class CreateCategoryCommand : IRequest<Result<CategoryResponse>> , IValidatable
    {
        public string Name { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _distributedCache;
        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper, IDistributedCache distributedCache)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _distributedCache = distributedCache;
        }

        public async Task<Result<CategoryResponse>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = _mapper.Map<Domain.Category>(request);
            await _categoryRepository.CreateAsync(category, cancellationToken);
            var changes = await _unitOfWork.SaveChangesAsync();
            if (changes <= 0)
            {
                return Result.Fail("Failed to create category.");
            }

            try
            {
                var removeListTask = _distributedCache.RemoveAsync($"{SD.Category}_List", cancellationToken);
                await Task.WhenAll(removeListTask);
            }
            catch (Exception)
            {

            }
            var response = _mapper.Map<CategoryResponse>(category);
            return Result.Ok(response);
        }
    }
}
