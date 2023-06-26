using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateItemCommand : IRequest<ItemDTO>
    {
        public CreateOrUpdateItemDTO Model { get; }
        public CreateItemCommand(CreateOrUpdateItemDTO model)
        {
            this.Model = model;
        }
    }

    public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, ItemDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateItemCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateItemCommandHandler(ILogger<CreateItemCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<ItemDTO> Handle(CreateItemCommand request, CancellationToken cancellationToken)
        {
            CreateOrUpdateItemDTO model = request.Model;

            var entity = _repository.Items.CreateProxy();
            {
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.Categories = model.Categories;
                entity.ColorCode = model.ColorCode;
            };

            _repository.Items.Add(entity);
            await _repository.CommitAsync();
            var itemDTO = _mapper.Map<ItemDTO>(entity);
            _cache.SetItem($"item_{entity.Id}", itemDTO);
            _cache.RemoveItem("items");
            return itemDTO;
        }
    }
}