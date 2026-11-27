using AutoMapper;
using E_Shop.Application.Common.Errors;
using E_Shop.Application.Common.Interfaces;
using E_Shop.Application.Features.Product.Queries;
using E_Shop.Application.PipelineBehaviors.Contracts;
using FluentResults;
using MediatR;

namespace E_Shop.Application.Features.Product.Commands
{
    public class UpdateProductCommand : IRequest<Result<ProductResponse>> , IValidatable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int CategoryId { get; set; }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateProductCommandHandler(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<ProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            bool categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken);
            if (!categoryExists)
            {
                return Result.Fail(new NotFoundError("Invalid category ID."));
            }

            var product = _mapper.Map<Domain.Product>(request);
            _productRepository.Update(product,cancellationToken);
            var changes = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (changes == 0)
            {
                return Result.Fail("Failed to update product.");
            }
            var response = _mapper.Map<ProductResponse>(product);
            return Result.Ok(response);
        }
    }
}
