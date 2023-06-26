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
    public class UpdateTodoItemCommand : IRequest<TodoItemDTO>
    {
        public int Id { get; init; }

        public string? Title { get; init; }

        public bool Done { get; init; }
    }

    public class UpdateTodoItemCommandHandler : IRequestHandler<UpdateTodoItemCommand, TodoItemDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateTodoItemCommandHandler> _logger;

        public UpdateTodoItemCommandHandler(ILogger<UpdateTodoItemCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<TodoItemDTO> IRequestHandler<UpdateTodoItemCommand, TodoItemDTO>.Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
        {
            var todoItemEntity = _repository.TodoItems.Get(request.Id);

            if (todoItemEntity == null)
            {
                throw new NotFoundException($"No TodoItem found for the Id {request.Id}");
            }

            todoItemEntity.Title = request.Title;
            todoItemEntity.Done = request.Done;

            _repository.TodoItems.Update(todoItemEntity);
            await _repository.CommitAsync();

            var todoItemDTO = _mapper.Map<TodoItemDTO>(todoItemEntity);
            _cache.RemoveItem($"todo_list_{todoItemDTO.ListId}");
            _cache.RemoveItem("todo_lists");
            return todoItemDTO;
        }
    }
}