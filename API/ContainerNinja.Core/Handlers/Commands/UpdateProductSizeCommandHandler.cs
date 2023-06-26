using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateProductSizeCommand : IRequest<WalmartProductDTO>
    {
        public int Id { get; init; }

        public float Size { get; init; }
    }

    public class UpdateProductSizeCommandHandler : IRequestHandler<UpdateProductSizeCommand, WalmartProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateProductSizeCommandHandler> _logger;

        public UpdateProductSizeCommandHandler(ILogger<UpdateProductSizeCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<WalmartProductDTO> IRequestHandler<UpdateProductSizeCommand, WalmartProductDTO>.Handle(UpdateProductSizeCommand request, CancellationToken cancellationToken)
        {
            var productEntity = _repository.WalmartProducts.Set.FirstOrDefault(p => p.Id == request.Id);

            if (productEntity == null)
            {
                throw new NotFoundException($"No Product found for the Id {request.Id}");
            }

            productEntity.Size = request.Size;
            _repository.WalmartProducts.Update(productEntity);
            await _repository.CommitAsync();

            _cache.RemoveItem("product_stocks");

            var walmartProductDTO = _mapper.Map<WalmartProductDTO>(productEntity);
            _cache.SetItem($"product_{request.Id}", walmartProductDTO);
            _cache.RemoveItem("products");

            return walmartProductDTO;
        }
    }
}