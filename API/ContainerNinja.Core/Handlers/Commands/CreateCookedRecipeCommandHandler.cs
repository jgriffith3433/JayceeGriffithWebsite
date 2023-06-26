using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateCookedRecipeCommand : IRequest<CookedRecipeDTO>
    {
        public int? RecipeId { get; init; }
    }

    public class CreateCookedRecipeCommandHandler : IRequestHandler<CreateCookedRecipeCommand, CookedRecipeDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCookedRecipeCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateCookedRecipeCommandHandler(ILogger<CreateCookedRecipeCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<CookedRecipeDTO> Handle(CreateCookedRecipeCommand request, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Set.Include(r => r.CookedRecipes).FirstOrDefault(r => r.Id == request.RecipeId);

            if (recipeEntity == null)
            {
                throw new NotFoundException($"No Recipe found for the Id {request.RecipeId}");
            }

            var cookedRecipeEntity = _repository.CookedRecipes.CreateProxy();
            cookedRecipeEntity.Recipe = recipeEntity;
            recipeEntity.CookedRecipes.Add(cookedRecipeEntity);
            foreach (var calledIngredient in recipeEntity.CalledIngredients)
            {
                var cookedRecipeCalledIngredient = _repository.CookedRecipeCalledIngredients.CreateProxy();
                {
                    cookedRecipeCalledIngredient.CookedRecipe = cookedRecipeEntity;
                    cookedRecipeCalledIngredient.CalledIngredient = calledIngredient;
                    cookedRecipeCalledIngredient.Name = calledIngredient.Name;
                    cookedRecipeCalledIngredient.KitchenUnitType = calledIngredient.KitchenUnitType;
                    cookedRecipeCalledIngredient.Amount = calledIngredient.Amount;
                    cookedRecipeCalledIngredient.KitchenProduct = calledIngredient.KitchenProduct;
                };
                cookedRecipeEntity.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
            }
            _repository.Recipes.Update(recipeEntity);
            _repository.CookedRecipes.Add(cookedRecipeEntity);

            await _repository.CommitAsync();

            var cookedRecipeDTO = _mapper.Map<CookedRecipeDTO>(cookedRecipeEntity);
            _cache.SetItem($"cooked_recipe_{cookedRecipeDTO.Id}", cookedRecipeDTO);
            _cache.RemoveItem("cooked_recipes");
            return cookedRecipeDTO;
        }
    }
}