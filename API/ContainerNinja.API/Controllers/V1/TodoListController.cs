using ContainerNinja.API.Filters;
using ContainerNinja.Contracts.Constants;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Handlers.Commands;
using ContainerNinja.Core.Handlers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ContainerNinja.API.Controllers.V1
{
    //[Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Admin}")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TodoListController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TodoListController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(TodoListDTO), (int)HttpStatusCode.Created)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<IActionResult> Post([FromBody] CreateOrUpdateTodoListDTO model)
        {
            var response = await _mediator.Send(new CreateTodoListCommand
            {
                Color = model.Color,
                Title = model.Title,
            });
            return StatusCode((int)HttpStatusCode.Created, response);
        }

        [MapToApiVersion("1.0")]
        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> Delete(int id)
        {
            return await _mediator.Send(new DeleteTodoListCommand
            {
                Id = id
            });
        }

        [MapToApiVersion("1.0")]
        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType(typeof(CreateOrUpdateTodoListDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<IActionResult> Update(int id, [FromBody] CreateOrUpdateTodoListDTO model)
        {
            var command = new UpdateTodoListCommand
            {
                Id = id,
                Title = model.Title,
            };
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(TodoListDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetTodoListByIdQuery(id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
}
