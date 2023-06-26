using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateCompletedOrderCommand : IRequest<int>
    {
        public string Name { get; init; }
        public string? UserImport { get; init; }
    }

    public class CreateCompletedOrderCommandHandler : IRequestHandler<CreateCompletedOrderCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCompletedOrderCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateCompletedOrderCommandHandler(ILogger<CreateCompletedOrderCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<int> Handle(CreateCompletedOrderCommand request, CancellationToken cancellationToken)
        {
            var completedOrderEntity = _repository.CompletedOrders.CreateProxy();
            completedOrderEntity.Name = request.Name;

            _repository.CompletedOrders.Add(completedOrderEntity);

            var imported = false;
            if (request.UserImport != null)
            {
                imported = true;
            }

            completedOrderEntity.UserImport = request.UserImport;

            await _repository.CommitAsync();

            if (imported)
            {
                //entity.AddDomainEvent(new CompletedOrderUserImportEvent(entity));
            }

            //await _repository.CommitAsync();

            var completedOrderDTO = _mapper.Map<CompletedOrderDTO>(completedOrderEntity);
            _cache.SetItem($"completed_order_{completedOrderDTO.Id}", completedOrderDTO);
            _cache.RemoveItem("completed_orders");
            return completedOrderDTO.Id;
        }
    }
}