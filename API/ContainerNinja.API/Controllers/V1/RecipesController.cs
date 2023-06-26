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
    public class RecipesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RecipesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        [ProducesResponseType(typeof(GetAllRecipesVM), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<GetAllRecipesVM>> Get()
        {
            var query = new GetAllRecipesQuery();
            return await _mediator.Send(query);
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(RecipeDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<RecipeDTO>> Create(CreateRecipeCommand command)
        {
            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("UpdateName/{id}")]
        [ProducesResponseType(typeof(RecipeDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<RecipeDTO>> UpdateName(int id, UpdateRecipeNameCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("UpdateServes/{id}")]
        [ProducesResponseType(typeof(RecipeDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<RecipeDTO>> UpdateServes(int id, UpdateRecipeServesCommand command)
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
        public async Task<ActionResult<int>> DeleteRecipe(int id)
        {
            return await _mediator.Send(new DeleteRecipeCommand
            {
                Id = id
            });
        }

        //Called Ingredients

        [MapToApiVersion("1.0")]
        [HttpGet("GetCalledIngredientDetails/{id}")]
        [ProducesResponseType(typeof(CalledIngredientDetailsDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<CalledIngredientDetailsDTO>> GetCalledIngredientDetails(int id)
        {
            return await _mediator.Send(new GetCalledIngredientDetailsQuery
            {
                Id = id
            });
        }

        [MapToApiVersion("1.0")]
        [HttpGet("SearchProductName")]
        [ProducesResponseType(typeof(CalledIngredientDetailsDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<CalledIngredientDetailsDTO>> SearchProductName([FromQuery] SearchCalledIngredientProductNameQuery query)
        {
            return await _mediator.Send(query);
        }

        [MapToApiVersion("1.0")]
        [HttpPost("CreateCalledIngredient")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> CreateCalledIngredient(CreateCalledIngredientCommand command)
        {
            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("UpdateCalledIngredient/{id}")]
        [ProducesResponseType(typeof(CalledIngredientDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<CalledIngredientDTO>> UpdateCalledIngredient(int id, UpdateCalledIngredientCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpPut("UpdateCalledIngredientDetails/{id}")]
        [ProducesResponseType(typeof(CalledIngredientDetailsDTO), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<CalledIngredientDetailsDTO>> UpdateCalledIngredientDetails(int id, UpdateCalledIngredientDetailsCommand command)
        {
            if (command == null || id != command.Id)
            {
                return BadRequest();
            }

            return await _mediator.Send(command);
        }

        [MapToApiVersion("1.0")]
        [HttpDelete("DeleteCalledIngredient/{id}")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(BaseResponseDTO))]
        public async Task<ActionResult<int>> DeleteCalledIngredient(int id)
        {
            return await _mediator.Send(new DeleteCalledIngredientCommand
            {
                Id = id
            });
        }
    }
}