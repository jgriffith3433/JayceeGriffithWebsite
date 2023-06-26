using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ContainerNinja.API.Filters;
using ContainerNinja.Contracts.Constants;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Handlers.Commands;
using ContainerNinja.Core.Handlers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContainerNinja.Controllers.V1
{
    //[Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Admin}")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CompletedOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompletedOrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        [ProducesResponseType(typeof(GetAllCompletedOrdersVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<IActionResult> Get()
        {
            var query = new GetAllCompletedOrdersQuery();
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [MapToApiVersion("1.0")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CompletedOrderDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<CompletedOrderDTO>> Get(int id)
        {
            return await _mediator.Send(new GetCompletedOrderQuery
            {
                Id = id
            });
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> Create(CreateCompletedOrderCommand command)
        {
            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CompletedOrderDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<CompletedOrderDTO>> Update(int id, UpdateCompletedOrderCommand command)
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
            return await _mediator.Send(new DeleteCompletedOrderCommand
            {
                Id = id
            });
        }


        //Completed Order Products

        [MapToApiVersion("1.0")]
        [HttpGet("GetCompletedOrderProduct/{id}")]
        [ProducesResponseType(typeof(CompletedOrderProductDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<CompletedOrderProductDTO>> GetCompletedOrderProduct(int id)
        {
            return await _mediator.Send(new GetCompletedOrderProductQuery
            {
                Id = id
            });
        }

        [MapToApiVersion("1.0")]
        [HttpGet("SearchCompletedOrderProductName")]
        [ProducesResponseType(typeof(CompletedOrderProductDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<CompletedOrderProductDTO>> SearchCompletedOrderProductName([FromQuery] SearchCompletedOrderProductNameQuery query)
        {
            return await _mediator.Send(query);
        }

        [MapToApiVersion("1.0")]
        [HttpPost("CreateCompletedOrderProduct")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> CreateCompletedOrderProduct(CreateCompletedOrderProductCommand command)
        {
            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("UpdateCompletedOrderProduct/{id}")]
        [ProducesResponseType(typeof(CompletedOrderProductDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<CompletedOrderProductDTO>> UpdateCompletedOrderProduct(int id, UpdateCompletedOrderProductCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpDelete("DeleteCompletedOrderProduct/{id}")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> DeleteCompletedOrderProduct(int id)
        {
            return await _mediator.Send(new DeleteCompletedOrderProductCommand
            {
                Id = id
            });
        }

    }
}