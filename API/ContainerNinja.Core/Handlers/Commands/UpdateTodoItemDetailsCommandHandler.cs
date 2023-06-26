using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class UpdateTodoItemDetailsCommand : IRequest<TodoItemDTO>
    {
        public int Id { get; init; }

        public int ListId { get; init; }

        public PriorityLevel Priority { get; init; }

        public string? Note { get; init; }
    }

    public class UpdateTodoItemDetailsCommandHandler : IRequestHandler<UpdateTodoItemDetailsCommand, TodoItemDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateTodoItemDetailsCommandHandler> _logger;

        public UpdateTodoItemDetailsCommandHandler(ILogger<UpdateTodoItemDetailsCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<TodoItemDTO> IRequestHandler<UpdateTodoItemDetailsCommand, TodoItemDTO>.Handle(UpdateTodoItemDetailsCommand request, CancellationToken cancellationToken)
        {
            var todoItemEntity = _repository.TodoItems.Get(request.Id);

            if (todoItemEntity == null)
            {
                throw new NotFoundException($"No TodoItem found for the Id {request.Id}");
            }

            todoItemEntity.ListId = request.ListId;
            todoItemEntity.Priority = request.Priority;
            todoItemEntity.Note = request.Note;

            _repository.TodoItems.Update(todoItemEntity);
            await _repository.CommitAsync();

            var todoItemDTO = _mapper.Map<TodoItemDTO>(todoItemEntity);

            _cache.RemoveItem($"todo_list_{todoItemDTO.ListId}");
            _cache.RemoveItem("todo_lists");

            return todoItemDTO;
        }
    }
}