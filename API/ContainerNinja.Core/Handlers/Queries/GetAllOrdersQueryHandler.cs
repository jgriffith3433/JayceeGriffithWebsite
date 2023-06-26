using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.ViewModels;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetAllOrdersQuery : IRequest<GetAllOrdersVM>
    {
    }

    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, GetAllOrdersVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetAllOrdersQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<GetAllOrdersVM> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<GetAllOrdersVM>("orders");

            if (cachedEntities == null)
            {
                var entities = await Task.FromResult(_repository.Orders.Set.AsEnumerable());
                var result = new GetAllOrdersVM
                {
                    Orders = _mapper.Map<List<OrderDTO>>(entities),
                };

                _cache.SetItem("orders", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }
        }
    }
}