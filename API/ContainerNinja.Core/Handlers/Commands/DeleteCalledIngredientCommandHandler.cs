using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteCalledIngredientCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteCalledIngredientCommandHandler : IRequestHandler<DeleteCalledIngredientCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteCalledIngredientCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteCalledIngredientCommand request, CancellationToken cancellationToken)
        {
            var calledIngredientEntity = _repository.CalledIngredients.Set.FirstOrDefault(p => p.Id == request.Id);

            if (calledIngredientEntity == null)
            {
                throw new NotFoundException($"No CalledIngredient found for the Id {request.Id}");
            }

            _cache.RemoveItem("called_ingredients");
            _cache.RemoveItem($"called_ingredient_{calledIngredientEntity.Id}");
            _cache.RemoveItem("recipes");
            _cache.RemoveItem($"recipe_{calledIngredientEntity.Recipe.Id}");

            calledIngredientEntity.Recipe.CalledIngredients.Remove(calledIngredientEntity);
            _repository.Recipes.Update(calledIngredientEntity.Recipe);

            _repository.CalledIngredients.Delete(calledIngredientEntity.Id);
            await _repository.CommitAsync();

            return request.Id;
        }
    }
}