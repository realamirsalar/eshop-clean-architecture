using AutoMapper;
using E_Shop.Application.Common.Errors;
using E_Shop.Application.Common.Interfaces;
using E_Shop.Application.PipelineBehaviors.Contracts;
using FluentResults;
using MediatR;

namespace E_Shop.Application.Features.Product.Queries
{
    public record GetProductByIdQuery(long Id) : IRequest<Result<ProductResponse>>, IValidatable;

    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                return Result.Fail(new NotFoundError($"Product with ID {request.Id} not found."));
            }
            var response = _mapper.Map<ProductResponse>(product);
            return Result.Ok(response);
        }
    }
}
