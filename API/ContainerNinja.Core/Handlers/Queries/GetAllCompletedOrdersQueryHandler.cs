using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using System.Text.Json;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetAllCompletedOrdersQuery : IRequest<GetAllCompletedOrdersVM>
    {
    }

    public class GetAllCompletedOrdersQueryHandler : IRequestHandler<GetAllCompletedOrdersQuery, GetAllCompletedOrdersVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetAllCompletedOrdersQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<GetAllCompletedOrdersVM> Handle(GetAllCompletedOrdersQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<GetAllCompletedOrdersVM>("completed_orders");

            if (cachedEntities == null)
            {
                var entities = await Task.FromResult(_repository.CompletedOrders.Set.AsEnumerable());
                var result = new GetAllCompletedOrdersVM
                {
                    CompletedOrders = _mapper.Map<List<CompletedOrderDTO>>(entities),
                };

                _cache.SetItem("completed_orders", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }
        }
    }
}