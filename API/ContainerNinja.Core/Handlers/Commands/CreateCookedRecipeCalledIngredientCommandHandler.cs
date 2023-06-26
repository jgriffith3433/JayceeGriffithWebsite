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
    public class CreateCookedRecipeCalledIngredientCommand : IRequest<int>
    {
        public string? Name { get; init; }
        public int CookedRecipeId { get; init; }
        public int? KitchenProductId { get; init; }
    }

    public class CreateCookedRecipeCalledIngredientCommandHandler : IRequestHandler<CreateCookedRecipeCalledIngredientCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCookedRecipeCalledIngredientCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateCookedRecipeCalledIngredientCommandHandler(ILogger<CreateCookedRecipeCalledIngredientCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<int> Handle(CreateCookedRecipeCalledIngredientCommand request, CancellationToken cancellationToken)
        {
            var cookedRecipeEntity = _repository.CookedRecipes.Get(request.CookedRecipeId);

            if (cookedRecipeEntity == null)
            {
                throw new NotFoundException($"No CookedRecipe found for the Id {request.CookedRecipeId}");
            }

            var cookedRecipeCalledIngredientEntity = _repository.CookedRecipeCalledIngredients.CreateProxy();
            {
                cookedRecipeCalledIngredientEntity.Name = request.Name;
                cookedRecipeCalledIngredientEntity.CookedRecipe = cookedRecipeEntity;
            };

            if (request.KitchenProductId.HasValue)
            {
                var kitchenProduct = _repository.KitchenProducts.Get(request.KitchenProductId.Value);

                if (kitchenProduct == null)
                {
                    throw new NotFoundException($"No KitchenProduct found for the Id {request.KitchenProductId}");
                }

                cookedRecipeCalledIngredientEntity.KitchenProduct = kitchenProduct;
            }

            cookedRecipeEntity.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredientEntity);
            _repository.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredientEntity);
            _repository.CookedRecipes.Update(cookedRecipeEntity);

            await _repository.CommitAsync();

            _cache.SetItem($"cooked_recipe_{cookedRecipeEntity.Id}", _mapper.Map<CookedRecipeDTO>(cookedRecipeEntity));
            _cache.SetItem($"cooked_recipe_called_ingredient_{cookedRecipeCalledIngredientEntity.Id}", _mapper.Map<CookedRecipeCalledIngredientDTO>(cookedRecipeCalledIngredientEntity));

            _cache.RemoveItem("cooked_recipes");
            _cache.RemoveItem("cooked_recipe_called_ingredients");

            return cookedRecipeCalledIngredientEntity.Id;
        }
    }
}