

using AutoMapper;
using E_Shop.Application.Common.Interfaces;
using FluentResults;
using MediatR;

namespace E_Shop.Application.Features.Product.Queries
{
    public record GetAllProductsQuery : IRequest<Result<List<ProductListResponse>>>;

    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<List<ProductListResponse>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetAllProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductListResponse>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetAllAsync(cancellationToken);

            var productResponses = _mapper.Map<List<ProductListResponse>>(products);

            return Result.Ok(productResponses);
        }
    }
}
