using Asp.Versioning;
using AutoMapper;
using E_Shop.Api.Contracts.Product;
using E_Shop.Application.Features.Product.Commands;
using E_Shop.Application.Features.Product.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace E_Shop.Api.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/products")]
    public class ProductController : ApiControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public ProductController(ISender sender, IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
        }
        [HttpPost]
        //[ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<CreateProductCommand>(request);
            var result = await _sender.Send(command, cancellationToken);
            return HandleResult(result);
        }
    }
}
