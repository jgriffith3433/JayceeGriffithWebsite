using ContainerNinja.Contracts.Constants;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.ViewModels;
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
    public class TodoListsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TodoListsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        [ProducesResponseType(typeof(GetAllTodoListsVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<GetAllTodoListsVM>> Get()
        {
            return await _mediator.Send(new GetAllTodoListsQuery());
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> Create(CreateTodoListCommand command)
        {
            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TodoListDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<TodoListDTO>> Update(int id, UpdateTodoListCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> Delete(int id)
        {
            return await _mediator.Send(new DeleteTodoListCommand
            {
                Id = id
            });
        }

        //TodoItems

        [MapToApiVersion("1.0")]
        [HttpPost("CreateTodoItem")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> CreateTodoItem(CreateTodoItemCommand command)
        {
            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("UpdateTodoItem/{id}")]
        [ProducesResponseType(typeof(TodoItemDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<TodoItemDTO>> UpdateTodoItem(int id, UpdateTodoItemCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("UpdateTodoItemDetails/{id}")]
        [ProducesResponseType(typeof(TodoItemDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<TodoItemDTO>> UpdateTodoItemDetails(int id, UpdateTodoItemDetailsCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpDelete("DeleteTodoItem/{id}")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> DeleteTodoItem(int id)
        {
            return await _mediator.Send(new DeleteTodoItemCommand
            {
                Id = id
            });
        }

        //[HttpGet]
        //public async Task<ActionResult<PaginatedList<TodoItemBriefDto>>> GetTodoItemsWithPagination([FromQuery] GetTodoItemsWithPaginationQuery query)
        //{
        //    return await Mediator.Send(query);
        //}
    }
}
