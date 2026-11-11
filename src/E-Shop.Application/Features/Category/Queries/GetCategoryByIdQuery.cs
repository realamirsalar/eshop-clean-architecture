using AutoMapper;
using E_Shop.Application.Common.Errors;
using E_Shop.Application.Common.Interfaces;
using E_Shop.Application.PipelineBehaviors.Contracts;
using FluentResults;
using MediatR;


namespace E_Shop.Application.Features.Category.Queries
{
    public record GetCategoryByIdQuery(long Id) : IRequest<Result<CategoryResponse>>, IValidatable;

    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }
        public async Task<Result<CategoryResponse>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                return Result.Fail(new NotFoundError($"Category with Id {request.Id} not found."));
            }
            var response = _mapper.Map<CategoryResponse>(category);
            return Result.Ok(response);
        }
    }
}
