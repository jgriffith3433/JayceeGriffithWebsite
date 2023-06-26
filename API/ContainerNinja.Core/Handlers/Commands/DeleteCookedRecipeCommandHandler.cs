using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteCookedRecipeCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteCookedRecipeCommandHandler : IRequestHandler<DeleteCookedRecipeCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteCookedRecipeCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteCookedRecipeCommand request, CancellationToken cancellationToken)
        {
            var cookedRecipeEntity = _repository.CookedRecipes.Set.FirstOrDefault(p => p.Id == request.Id);

            if (cookedRecipeEntity == null)
            {
                throw new NotFoundException($"No CookedRecipe found for the Id {request.Id}");
            }

            foreach(var cookedRecipeCalledIngredient in cookedRecipeEntity.CookedRecipeCalledIngredients)
            {
                _cache.RemoveItem("cooked_recipe_called_ingredients");
                _cache.RemoveItem($"cooked_recipe_called_ingredient_{cookedRecipeCalledIngredient.Id}");
                _repository.CookedRecipeCalledIngredients.Delete(cookedRecipeCalledIngredient.Id);
            }

            _cache.RemoveItem("cooked_recipes");
            _cache.RemoveItem($"cooked_recipe_{request.Id}");
            _repository.CookedRecipes.Delete(request.Id);

            await _repository.CommitAsync();

            return cookedRecipeEntity.Id;
        }
    }
}