using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateRecipeServesCommand : IRequest<RecipeDTO>
    {
        public int Id { get; init; }

        public int? Serves { get; init; }
    }

    public class UpdateRecipeServesCommandHandler : IRequestHandler<UpdateRecipeServesCommand, RecipeDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateRecipeServesCommandHandler> _logger;

        public UpdateRecipeServesCommandHandler(ILogger<UpdateRecipeServesCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<RecipeDTO> IRequestHandler<UpdateRecipeServesCommand, RecipeDTO>.Handle(UpdateRecipeServesCommand request, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Get(request.Id);

            if (recipeEntity == null)
            {
                throw new NotFoundException($"No Recipe found for the Id {request.Id}");
            }

            if (recipeEntity.Serves != request.Serves)
            {
                recipeEntity.Serves = request.Serves;
                _repository.Recipes.Update(recipeEntity);
                await _repository.CommitAsync();
            }

            var recipeDTO = _mapper.Map<RecipeDTO>(recipeEntity);
            _cache.SetItem($"recipe_{request.Id}", recipeDTO);
            _cache.RemoveItem("recipes");

            return recipeDTO;
        }
    }
}