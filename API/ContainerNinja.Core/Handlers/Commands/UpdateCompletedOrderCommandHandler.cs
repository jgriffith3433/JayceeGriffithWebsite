using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateCompletedOrderCommand : IRequest<CompletedOrderDTO>
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public string? UserImport { get; init; }
    }

    public class UpdateCompletedOrderCommandHandler : IRequestHandler<UpdateCompletedOrderCommand, CompletedOrderDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateCompletedOrderCommandHandler> _logger;

        public UpdateCompletedOrderCommandHandler(ILogger<UpdateCompletedOrderCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<CompletedOrderDTO> IRequestHandler<UpdateCompletedOrderCommand, CompletedOrderDTO>.Handle(UpdateCompletedOrderCommand request, CancellationToken cancellationToken)
        {
            var completedOrderEntity = _repository.CompletedOrders.Get(request.Id);

            if (completedOrderEntity == null)
            {
                throw new NotFoundException($"No Completed Order found for the Id {request.Id}");
            }

            completedOrderEntity.Name = request.Name;
            var imported = false;
            if (request.UserImport != null && completedOrderEntity.UserImport != request.UserImport)
            {
                imported = true;
            }

            completedOrderEntity.UserImport = request.UserImport;
            _repository.CompletedOrders.Update(completedOrderEntity);

            await _repository.CommitAsync();

            if (imported)
            {
                //completedOrderEntity.AddDomainEvent(new CompletedOrderUserImportEvent(completedOrderEntity));
            }

            //await _repository.CommitAsync();


            var completedOrderDTO = _mapper.Map<CompletedOrderDTO>(completedOrderEntity);
            _cache.SetItem($"completed_order_{request.Id}", completedOrderDTO);
            _cache.RemoveItem("completed_orders");

            return completedOrderDTO;
        }
    }
}