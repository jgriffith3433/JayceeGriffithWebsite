using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using Alachisoft.NCache.Client;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetKitchenProductDetailsQuery : IRequest<KitchenProductDetailsDTO>
    {
        public int Id { get; init; }
        public string Name { get; init; }
    }

    public class GetKitchenProductDetailsQueryHandler : IRequestHandler<GetKitchenProductDetailsQuery, KitchenProductDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetKitchenProductDetailsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<KitchenProductDetailsDTO> Handle(GetKitchenProductDetailsQuery request, CancellationToken cancellationToken)
        {
            var kitchenProductDTO = _cache.GetItem<KitchenProductDTO>($"product_stock_{request.Id}");
            if (kitchenProductDTO == null)
            {
                var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(ps => ps.Id == request.Id);

                if (kitchenProductEntity == null)
                {
                    throw new NotFoundException($"No KitchenProduct found for the Id {request.Id}");
                }

                kitchenProductDTO = _mapper.Map<KitchenProductDTO>(kitchenProductEntity);
                _cache.SetItem($"product_stock_{request.Id}", kitchenProductDTO);
            }

            var kitchenProductDetailsDTO = _mapper.Map<KitchenProductDetailsDTO>(kitchenProductDTO);

            //TODO: needs to be looked at after relationship change
            //var searchResults = from p in _repository.WalmartProducts.Set where EF.Functions.Like(p.Name, string.Format("%{0}%", request.Name)) || p.Id == kitchenProductDTO.ProductId select p;

            //kitchenProductDetailsDTO.ProductSearchItems = _mapper.Map<IEnumerable<WalmartProductDTO>>(searchResults).ToList();

            //foreach (var productSearchItem in kitchenProductDetailsDTO.ProductSearchItems)
            //{
            //    if (productSearchItem.KitchenProductId == request.Id)
            //    {
            //        productSearchItem.Name += " ( Linked )";
            //    }
            //    else if (productSearchItem.KitchenProductId != null)
            //    {
            //        productSearchItem.Name += " ( Merge )";
            //    }
            //}
            return kitchenProductDetailsDTO;
        }
    }
}