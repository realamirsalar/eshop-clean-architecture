

using AutoMapper;
using E_Shop.Application.Common.Interfaces;
using FluentResults;
using MediatR;

namespace E_Shop.Application.Features.Category.Queries
{
    public record GetAllCategoriesQuery : IRequest<Result<List<CategoryResponse>>> ;

    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryResponse>>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<CategoryResponse>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);

            var result = _mapper.Map<List<CategoryResponse>>(categories);

            return Result.Ok(result);
        }
    }
}
