using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class UpdateTodoListCommand : IRequest<TodoListDTO>
    {
        public int Id { get; init; }

        public string? Title { get; init; }
    }

    public class UpdateTodoListCommandHandler : IRequestHandler<UpdateTodoListCommand, TodoListDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateTodoListCommandHandler> _logger;

        public UpdateTodoListCommandHandler(ILogger<UpdateTodoListCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<TodoListDTO> IRequestHandler<UpdateTodoListCommand, TodoListDTO>.Handle(UpdateTodoListCommand request, CancellationToken cancellationToken)
        {
            var todoListEntity = _repository.TodoLists.Set.FirstOrDefault(tdl => tdl.Id == request.Id);

            if (todoListEntity == null)
            {
                throw new NotFoundException($"No TodoList found for the Id {request.Id}");
            }

            todoListEntity.Title = request.Title;

            _repository.TodoLists.Update(todoListEntity);
            await _repository.CommitAsync();

            var todoListDTO = _mapper.Map<TodoListDTO>(todoListEntity);
            _cache.SetItem($"todo_list_{request.Id}", todoListDTO);
            _cache.RemoveItem("todo_lists");
            return todoListDTO;
        }
    }
}