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
    public class CookedRecipesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CookedRecipesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        [ProducesResponseType(typeof(GetAllCookedRecipesVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<GetAllCookedRecipesVM>> Get()
        {
            var query = new GetAllCookedRecipesQuery();
            return await _mediator.Send(query);
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(CookedRecipeDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<CookedRecipeDTO>> Create(CreateCookedRecipeCommand command)
        {
            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> Delete(int id)
        {
            return await _mediator.Send(new DeleteCookedRecipeCommand
            {
                Id = id
            });
        }

        //CookedRecipeCalledIngredients
        [HttpGet("GetCookedRecipeCalledIngredientDetails/{id}")]
        public async Task<ActionResult<CookedRecipeCalledIngredientDetailsDTO>> GetCookedRecipeCalledIngredientDetails(int id)
        {
            return await _mediator.Send(new GetCookedRecipeCalledIngredientDetailsQuery
            {
                Id = id
            });
        }

        [HttpGet("SearchProductName")]
        public async Task<ActionResult<CookedRecipeCalledIngredientDetailsDTO>> SearchProductName([FromQuery] SearchCookedRecipeCalledIngredientProductNameQuery query)
        {
            return await _mediator.Send(query);
        }

        [HttpPost("CreateCookedRecipeCalledIngredient")]
        public async Task<ActionResult<int>> CreateCookedRecipeCalledIngredient(CreateCookedRecipeCalledIngredientCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPut("UpdateCookedRecipeCalledIngredient/{id}")]
        public async Task<ActionResult<CookedRecipeCalledIngredientDTO>> UpdateCookedRecipeCalledIngredient(int id, UpdateCookedRecipeCalledIngredientCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [HttpPut("UpdateCookedRecipeCalledIngredientDetails/{id}")]
        public async Task<ActionResult<CookedRecipeCalledIngredientDetailsDTO>> UpdateCookedRecipeCalledIngredientDetails(int id, UpdateCookedRecipeCalledIngredientDetailsCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [HttpDelete("DeleteCookedRecipeCalledIngredient/{id}")]
        public async Task<ActionResult<int>> DeleteCookedRecipeCalledIngredient(int id)
        {
            return await _mediator.Send(new DeleteCookedRecipeCalledIngredientCommand
            {
                Id = id
            });
        }
    }
}