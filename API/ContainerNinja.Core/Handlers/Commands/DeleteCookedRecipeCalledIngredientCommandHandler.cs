using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteCookedRecipeCalledIngredientCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteCookedRecipeCalledIngredientCommandHandler : IRequestHandler<DeleteCookedRecipeCalledIngredientCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteCookedRecipeCalledIngredientCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteCookedRecipeCalledIngredientCommand request, CancellationToken cancellationToken)
        {
            var cookedRecipeCalledIngredientEntity = _repository.CookedRecipeCalledIngredients.Set.FirstOrDefault(p => p.Id == request.Id);

            if (cookedRecipeCalledIngredientEntity == null)
            {
                throw new NotFoundException($"No CookedRecipeCalledIngredient found for the Id {request.Id}");
            }

            _cache.RemoveItem("cooked_recipe_called_ingredients");
            _cache.RemoveItem($"cooked_recipe_called_ingredient_{cookedRecipeCalledIngredientEntity.Id}");
            _cache.RemoveItem("cooked_recipes");
            _cache.RemoveItem($"cooked_recipe_{cookedRecipeCalledIngredientEntity.CookedRecipe.Id}");

            cookedRecipeCalledIngredientEntity.CookedRecipe.CookedRecipeCalledIngredients.Remove(cookedRecipeCalledIngredientEntity);
            _repository.CookedRecipes.Update(cookedRecipeCalledIngredientEntity.CookedRecipe);

            _repository.CookedRecipeCalledIngredients.Delete(cookedRecipeCalledIngredientEntity.Id);
            await _repository.CommitAsync();

            return request.Id;
        }
    }
}