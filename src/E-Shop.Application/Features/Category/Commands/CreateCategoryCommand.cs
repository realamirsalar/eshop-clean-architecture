using AutoMapper;
using E_Shop.Application.Common.Interfaces;
using E_Shop.Application.Features.Category.Queries;
using E_Shop.Domain;
using FluentResults;
using MediatR;

namespace E_Shop.Application.Features.Category.Commands
{
    public class CreateCategoryCommand : IRequest<Result<CategoryResponse>>
    {
        public string Name { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
            var response = _mapper.Map<CategoryResponse>(category);
            return Result.Ok(response);
        }
    }
}
