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
    public class CreateTodoListCommand : IRequest<int>
    {
        public string Title { get; set; }
        public string Color { get; set; }
    }

    public class CreateTodoListCommandHandler : IRequestHandler<CreateTodoListCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateTodoListCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateTodoListCommandHandler(ILogger<CreateTodoListCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<int> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
        {
            var entity = _repository.TodoLists.CreateProxy();
            {
                entity.Title = request.Title;
                entity.Color = request.Color;
            };

            _repository.TodoLists.Add(entity);
            await _repository.CommitAsync();

            var todoListDTO = _mapper.Map<TodoListDTO>(entity);
            _cache.SetItem($"todo_list_{entity.Id}", todoListDTO);
            _cache.RemoveItem("todo_lists");
            return todoListDTO.Id;
        }
    }
}