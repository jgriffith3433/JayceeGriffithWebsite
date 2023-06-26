using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetCompletedOrderQuery : IRequest<CompletedOrderDTO>
    {
        public int Id { get; init; }
    }

    public class GetCompletedOrderQueryHandler : IRequestHandler<GetCompletedOrderQuery, CompletedOrderDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetCompletedOrderQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<CompletedOrderDTO> Handle(GetCompletedOrderQuery request, CancellationToken cancellationToken)
        {
            var completedOrderDTO = _cache.GetItem<CompletedOrderDTO>($"completed_order_{request.Id}");
            if (completedOrderDTO == null)
            {
                var completedOrderEntity = _repository.CompletedOrders.Set.FirstOrDefault(co => co.Id == request.Id);

                if (completedOrderEntity == null)
                {
                    throw new NotFoundException($"No CompletedOrder found for the Id {request.Id}");
                }

                completedOrderDTO = _mapper.Map<CompletedOrderDTO>(completedOrderEntity);
                _cache.SetItem($"completed_order_{request.Id}", completedOrderDTO);
            }

            return completedOrderDTO;
        }
    }
}