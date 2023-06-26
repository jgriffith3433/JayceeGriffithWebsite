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
    public class KitchenProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public KitchenProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        [ProducesResponseType(typeof(GetAllKitchenProductsVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<GetAllKitchenProductsVM>> Get()
        {
            var query = new GetAllKitchenProductsQuery();
            return await _mediator.Send(query);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(KitchenProductDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<KitchenProductDTO>> Update(int id, UpdateKitchenProductCommand command)
        {
            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpGet("GetKitchenProductDetails")]
        [ProducesResponseType(typeof(KitchenProductDetailsDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<KitchenProductDetailsDTO>> GetKitchenProductDetails([FromQuery] GetKitchenProductDetailsQuery query)
        {
            return await _mediator.Send(query);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("UpdateKitchenProductDetails/{id}")]
        [ProducesResponseType(typeof(KitchenProductDetailsDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<KitchenProductDetailsDTO>> UpdateKitchenProductDetails(int id, UpdateKitchenProductDetailsCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> Create(CreateKitchenProductCommand command)
        {
            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> Delete(int id)
        {
            return await _mediator.Send(new DeleteKitchenProductCommand
            {
                Id = id
            });
        }
    }
}