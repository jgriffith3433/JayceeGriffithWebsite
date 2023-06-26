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
    public class CreateRecipeCommand : IRequest<RecipeDTO>
    {
        public string Name { get; init; }
        public int? Serves { get; init; }
        public string? UserImport { get; init; }
    }

    public class CreateRecipeCommandHandler : IRequestHandler<CreateRecipeCommand, RecipeDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateRecipeCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateRecipeCommandHandler(ILogger<CreateRecipeCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<RecipeDTO> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.CreateProxy();
            {
                recipeEntity.Name = request.Name;
                recipeEntity.Serves = request.Serves.Value;
                recipeEntity.UserImport = request.UserImport;
            };

            _repository.Recipes.Add(recipeEntity);

            //if (entity.UserImport != null)
            //{
            //    entity.AddDomainEvent(new RecipeUserImportEvent(entity));
            //}

            await _repository.CommitAsync();

            var recipeDTO = _mapper.Map<RecipeDTO>(recipeEntity);
            _cache.SetItem($"recipe_{recipeDTO.Id}", recipeDTO);
            _cache.RemoveItem("recipes");
            return recipeDTO;
        }
    }
}