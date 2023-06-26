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
    public class UpdateItemCommand : IRequest<ItemDTO>
    {
        public int ItemId { get; set; }
        public CreateOrUpdateItemDTO Model { get; }

        public UpdateItemCommand(int itemId, CreateOrUpdateItemDTO model)
        {
            this.ItemId = itemId;
            this.Model = model;
        }
    }

    public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, ItemDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateItemCommandHandler> _logger;

        public UpdateItemCommandHandler(ILogger<UpdateItemCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<ItemDTO> IRequestHandler<UpdateItemCommand, ItemDTO>.Handle(UpdateItemCommand request, CancellationToken cancellationToken)
        {
            CreateOrUpdateItemDTO model = request.Model;
            int itemId = request.ItemId;

            var dbEntity = _repository.Items.Get(itemId);

            if (dbEntity == null)
            {
                throw new NotFoundException($"No Item found for the Id {itemId}");
            }

            dbEntity.Name = model.Name;
            dbEntity.Description = model.Description;
            dbEntity.Categories = model.Categories;
            dbEntity.ColorCode = model.ColorCode;

            _repository.Items.Update(dbEntity);
            await _repository.CommitAsync();

            var updatedItem = _mapper.Map<ItemDTO>(dbEntity);
            
            _cache.SetItem($"item_{itemId}", updatedItem);
            _cache.RemoveItem("items");
            
            return updatedItem;
        }
    }
}