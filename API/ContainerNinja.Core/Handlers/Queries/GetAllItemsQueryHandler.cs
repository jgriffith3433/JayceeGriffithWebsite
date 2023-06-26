using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using System.Text.Json;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetAllItemsQuery : IRequest<IEnumerable<ItemDTO>>
    {
    }

    public class GetAllItemsQueryHandler : IRequestHandler<GetAllItemsQuery, IEnumerable<ItemDTO>>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetAllItemsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<IEnumerable<ItemDTO>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<IEnumerable<ItemDTO>>("items");
            
            if (cachedEntities == null)
            {
                var entities = await Task.FromResult(_repository.Items.GetAll());
                var result = _mapper.Map<IEnumerable<ItemDTO>>(entities);

                _cache.SetItem("items", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }

        }
    }
}