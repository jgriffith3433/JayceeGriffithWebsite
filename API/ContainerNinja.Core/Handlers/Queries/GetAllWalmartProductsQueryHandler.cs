using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using System.Text.Json;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetAllWalmartProductsQuery : IRequest<GetAllWalmartProductsVM>
    {
    }

    public class GetAllWalmartProductsQueryHandler : IRequestHandler<GetAllWalmartProductsQuery, GetAllWalmartProductsVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetAllWalmartProductsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<GetAllWalmartProductsVM> Handle(GetAllWalmartProductsQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<GetAllWalmartProductsVM>("products");

            if (cachedEntities == null)
            {
                var entities = await Task.FromResult(_repository.WalmartProducts.GetAll());
                var result = new GetAllWalmartProductsVM
                {
                    WalmartProducts = _mapper.Map<List<WalmartProductDTO>>(entities),
                    KitchenUnitTypes = Enum.GetValues(typeof(KitchenUnitType))
                    .Cast<KitchenUnitType>()
                    .Select(p => new KitchenUnitTypeDTO { Value = (int)p, Name = p.ToString() })
                    .ToList(),
                };

                _cache.SetItem("products", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }

        }
    }
}