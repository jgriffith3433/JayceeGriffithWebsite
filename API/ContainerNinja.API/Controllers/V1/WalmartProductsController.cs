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
    public class WalmartProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WalmartProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        [ProducesResponseType(typeof(GetAllWalmartProductsVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<GetAllWalmartProductsVM>> Get()
        {
            var query = new GetAllWalmartProductsQuery();
            return await _mediator.Send(query);
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> Create(CreateWalmartProductCommand command)
        {
            return await _mediator.Send(command);
        }


        [MapToApiVersion("1.0")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(WalmartProductDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<WalmartProductDTO>> Update(int id, UpdateProductCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }


        [MapToApiVersion("1.0")]
        [HttpPut("UpdateProductName/{id}")]
        [ProducesResponseType(typeof(WalmartProductDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<WalmartProductDTO>> UpdateProductName(int id, UpdateProductNameCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }


        [MapToApiVersion("1.0")]
        [HttpPut("UpdateKitchenUnitType/{id}")]
        [ProducesResponseType(typeof(WalmartProductDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<WalmartProductDTO>> UpdateKitchenUnitType(int id, UpdateProductKitchenUnitTypeCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("UpdateSize/{id}")]
        [ProducesResponseType(typeof(WalmartProductDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<WalmartProductDTO>> UpdateSize(int id, UpdateProductSizeCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpGet("GetProductDetails")]
        [ProducesResponseType(typeof(WalmartProductDetailsDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<WalmartProductDetailsDTO>> GetProductDetails([FromQuery] GetProductDetailsQuery query)
        {
            return await _mediator.Send(query);
        }

        [MapToApiVersion("1.0")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> Delete(int id)
        {
            return await _mediator.Send(new DeleteWalmartProductCommand
            {
                Id = id
            });
        }
    }
}