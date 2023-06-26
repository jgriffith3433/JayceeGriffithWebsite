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
    public class CreateTodoItemCommand : IRequest<int>
    {
        public int ListId { get; init; }

        public string? Title { get; init; }
    }

    public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateTodoItemCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateTodoItemCommandHandler(ILogger<CreateTodoItemCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
        {
            var entity = _repository.TodoItems.CreateProxy();
            {
                entity.Title = request.Title;
                entity.ListId = request.ListId;
            };

            _repository.TodoItems.Add(entity);
            await _repository.CommitAsync();
            _cache.RemoveItem($"todo_list_{entity.ListId}");
            _cache.RemoveItem("todo_lists");
            return entity.Id;
        }
    }
}