using Asp.Versioning;
using AutoMapper;
using E_Shop.Api.Contracts.Category;
using E_Shop.Application.Features.Category.Commands;
using E_Shop.Application.Features.Category.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace E_Shop.Api.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/categories")]
    public class CategoryController : ApiControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public CategoryController(ISender sender, IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponse),StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
        {
            var command = _mapper.Map<CreateCategoryCommand>(request);
            var result = await _sender.Send(command);
            if(result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetCategoryById), new { id = result.Value.Id }, result.Value);
            }
            return HandleResult(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetAllCategories()
        {
            var result = await _sender.Send(new GetAllCategoriesQuery());
            return HandleResult(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetCategoryById([FromRoute] int id)
        {
            var result = await _sender.Send(new GetCategoryByIdQuery(id));
            return HandleResult(result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(CategoryResponse),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> UpdateCategory([FromRoute]int id,[FromBody] UpdateCategoryRequest request)
        {
            request.Id = id;
            var command = _mapper.Map<UpdateCategoryCommand>(request);
            var result = await _sender.Send(command);
            return HandleResult(result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id)
        {
            var command = _mapper.Map<DeleteCategoryCommand>(new DeleteCategoryCommand(id));
            var result = await _sender.Send(command);
            return HandleResult(result);
        }

    }
}
