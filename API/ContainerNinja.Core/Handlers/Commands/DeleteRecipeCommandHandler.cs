using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteRecipeCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteRecipeCommandHandler : IRequestHandler<DeleteRecipeCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteRecipeCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Set.FirstOrDefault(p => p.Id == request.Id);

            if (recipeEntity == null)
            {
                throw new NotFoundException($"No Recipe found for the Id {request.Id}");
            }

            foreach(var calledIngredient in recipeEntity.CalledIngredients)
            {
                _cache.RemoveItem("called_ingredients");
                _cache.RemoveItem($"called_ingredient_{calledIngredient.Id}");
                _repository.CalledIngredients.Delete(calledIngredient.Id);
            }

            _cache.RemoveItem("recipes");
            _cache.RemoveItem($"recipe_{request.Id}");
            _repository.Recipes.Delete(request.Id);

            await _repository.CommitAsync();

            return recipeEntity.Id;
        }
    }
}