using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateCookedRecipeCalledIngredientDetailsCommand : IRequest<CookedRecipeCalledIngredientDetailsDTO>
    {
        public int Id { get; init; }

        public string? Name { get; init; }

        public float? Amount { get; init; }

        public int? KitchenProductId { get; init; }
    }

    public class UpdateCookedRecipeCalledIngredientDetailsCommandHandler : IRequestHandler<UpdateCookedRecipeCalledIngredientDetailsCommand, CookedRecipeCalledIngredientDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateCookedRecipeCalledIngredientDetailsCommandHandler> _logger;

        public UpdateCookedRecipeCalledIngredientDetailsCommandHandler(ILogger<UpdateCookedRecipeCalledIngredientDetailsCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<CookedRecipeCalledIngredientDetailsDTO> IRequestHandler<UpdateCookedRecipeCalledIngredientDetailsCommand, CookedRecipeCalledIngredientDetailsDTO>.Handle(UpdateCookedRecipeCalledIngredientDetailsCommand request, CancellationToken cancellationToken)
        {
            var cookedRecipeCalledIngredientEntity = _repository.CookedRecipeCalledIngredients.Set.FirstOrDefault(co => co.Id == request.Id);

            if (cookedRecipeCalledIngredientEntity == null)
            {
                throw new NotFoundException($"No CookedRecipeCalledIngredient found for the Id {request.Id}");
            }

            cookedRecipeCalledIngredientEntity.Name = request.Name;
            cookedRecipeCalledIngredientEntity.Amount = request.Amount;

            if (request.KitchenProductId.HasValue)
            {
                var kitchenProduct = _repository.KitchenProducts.Get(request.KitchenProductId.Value);

                if (kitchenProduct == null)
                {
                    throw new NotFoundException($"No KitchenProduct found for the Id {request.KitchenProductId}");
                }
                cookedRecipeCalledIngredientEntity.KitchenProduct = kitchenProduct;
            }
            _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredientEntity);
            await _repository.CommitAsync();

            var cookedRecipeCalledIngredientDTO = _mapper.Map<CookedRecipeCalledIngredientDTO>(cookedRecipeCalledIngredientEntity);
            _cache.SetItem($"cooked_recipe_called_ingredient_{request.Id}", cookedRecipeCalledIngredientDTO);
            _cache.RemoveItem("cooked_recipe_called_ingredients");

            var cookedRecipeCalledIngredientDetailsDTO = _mapper.Map<CookedRecipeCalledIngredientDetailsDTO>(cookedRecipeCalledIngredientDTO);

            var query = from ps in _repository.KitchenProducts.Set where EF.Functions.Like(ps.Name, string.Format("%{0}%", cookedRecipeCalledIngredientDetailsDTO.Name))
                        select ps;

            cookedRecipeCalledIngredientDetailsDTO.KitchenProductSearchItems = _mapper.Map<List<KitchenProductDTO>>(query.ToList());

            return cookedRecipeCalledIngredientDetailsDTO;
        }
    }
}