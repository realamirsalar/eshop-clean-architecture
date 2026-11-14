

using AutoMapper;
using E_Shop.Application.Common.Interfaces;
using E_Shop.Application.Features.Product.Queries;
using E_Shop.Application.PipelineBehaviors.Contracts;
using FluentResults;
using MediatR;

namespace E_Shop.Application.Features.Product.Commands
{
    public class CreateProductCommand : IRequest<Result<ProductResponse>> , IValidatable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int CategoryId { get; set; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Domain.Product>(request);
            await _productRepository.AddAsync(product);
            var changes = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (changes == 0)
            {
                return Result.Fail("Failed to create product.");
            }
            var response = _mapper.Map<ProductResponse>(product);
            return Result.Ok(response);
        }
    }
}
