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
    public class GetAllKitchenProductsQuery : IRequest<GetAllKitchenProductsVM>
    {
    }

    public class GetAllKitchenProductsQueryHandlerHandler : IRequestHandler<GetAllKitchenProductsQuery, GetAllKitchenProductsVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetAllKitchenProductsQueryHandlerHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<GetAllKitchenProductsVM> Handle(GetAllKitchenProductsQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<GetAllKitchenProductsVM>("product_stocks");

            if (cachedEntities == null)
            {
                var entities = _repository.KitchenProducts.Set.AsEnumerable();
                var result = new GetAllKitchenProductsVM
                {
                    KitchenProducts = _mapper.Map<List<KitchenProductDTO>>(entities),
                    KitchenUnitTypes = Enum.GetValues(typeof(KitchenUnitType))
                    .Cast<KitchenUnitType>()
                    .Select(p => new KitchenUnitTypeDTO { Value = (int)p, Name = p.ToString() })
                    .ToList(),
                };

                _cache.SetItem("product_stocks", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }
        }
    }
}