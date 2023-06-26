using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Services;
using System.Text.Json;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class SearchCompletedOrderProductNameQuery : IRequest<CompletedOrderProductDTO>
    {
        public int Id { get; init; }
        public string Name { get; init; }
    }

    public class SearchCompletedOrderProductNameQueryHandler : IRequestHandler<SearchCompletedOrderProductNameQuery, CompletedOrderProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IWalmartService _walmartService;

        public SearchCompletedOrderProductNameQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache, IWalmartService walmartService)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _walmartService = walmartService;
        }

        public async Task<CompletedOrderProductDTO> Handle(SearchCompletedOrderProductNameQuery request, CancellationToken cancellationToken)
        {
            var completedOrderProductEntity = _repository.CompletedOrderProducts.Set.FirstOrDefault(cop => cop.Id == request.Id);
            if (completedOrderProductEntity == null)
            {
                throw new NotFoundException($"No CompletedOrderProduct found for the Id {request.Id}");
            }

            var searchResponse = await _walmartService.Search(request.Name);
            completedOrderProductEntity.WalmartSearchResponse = JsonSerializer.Serialize(searchResponse);

            _repository.CompletedOrderProducts.Update(completedOrderProductEntity);
            await _repository.CommitAsync();

            var completedOrderProductDTO = _mapper.Map<CompletedOrderProductDTO>(completedOrderProductEntity);
            _cache.SetItem($"completed_order_product_{request.Id}", completedOrderProductDTO);

            return completedOrderProductDTO;
        }
    }
}