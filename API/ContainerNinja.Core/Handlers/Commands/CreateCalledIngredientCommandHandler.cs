using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateCalledIngredientCommand : IRequest<int>
    {
        public string? Name { get; init; }
        public int RecipeId { get; init; }
    }

    public class CreateCalledIngredientCommandHandler : IRequestHandler<CreateCalledIngredientCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCalledIngredientCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateCalledIngredientCommandHandler(ILogger<CreateCalledIngredientCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<int> Handle(CreateCalledIngredientCommand request, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Get(request.RecipeId);

            if (recipeEntity == null)
            {
                throw new NotFoundException($"No Recipe found for the Id {request.RecipeId}");
            }

            var calledIngredientEntity = _repository.CalledIngredients.CreateProxy();
            {
                calledIngredientEntity.Name = request.Name;
                calledIngredientEntity.Recipe = recipeEntity;
            };

            recipeEntity.CalledIngredients.Add(calledIngredientEntity);
            _repository.CalledIngredients.Add(calledIngredientEntity);
            _repository.Recipes.Update(recipeEntity);

            await _repository.CommitAsync();

            _cache.SetItem($"recipe_{recipeEntity.Id}", _mapper.Map<RecipeDTO>(recipeEntity));
            _cache.SetItem($"called_ingredient_{calledIngredientEntity.Id}", _mapper.Map<CalledIngredientDTO>(calledIngredientEntity));

            _cache.RemoveItem("recipes");
            _cache.RemoveItem("called_ingredients");

            return calledIngredientEntity.Id;
        }
    }
}