using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetProductDetailsQuery : IRequest<WalmartProductDetailsDTO>
    {
        public int Id { get; init; }
    }

    public class GetWalmartProductDetailsQueryHandler : IRequestHandler<GetProductDetailsQuery, WalmartProductDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetWalmartProductDetailsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<WalmartProductDetailsDTO> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
        {
            var walmartProductDTO = _cache.GetItem<WalmartProductDTO>($"product_{request.Id}");
            if (walmartProductDTO == null)
            {
                var productEntity = _repository.WalmartProducts.Get(request.Id);

                if (productEntity == null)
                {
                    throw new NotFoundException($"No Product found for the Id {request.Id}");
                }

                walmartProductDTO = _mapper.Map<WalmartProductDTO>(productEntity);
                _cache.SetItem($"product_{request.Id}", walmartProductDTO);
            }

            var productDetailsDTO = _mapper.Map<WalmartProductDetailsDTO>(walmartProductDTO);

            return productDetailsDTO;
        }
    }
}