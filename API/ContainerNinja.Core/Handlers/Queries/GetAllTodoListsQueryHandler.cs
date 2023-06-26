using AutoMapper;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Queries
{
    public class GetAllTodoListsQuery : IRequest<GetAllTodoListsVM>
    {
    }

    public class GetAllTodoListsQueryHandler : IRequestHandler<GetAllTodoListsQuery, GetAllTodoListsVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public GetAllTodoListsQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<GetAllTodoListsVM> Handle(GetAllTodoListsQuery request, CancellationToken cancellationToken)
        {
            var cachedEntities = _cache.GetItem<GetAllTodoListsVM>("todo_lists");

            if (cachedEntities == null)
            {
                var entities = _repository.TodoLists.Set.AsEnumerable();
                var result = new GetAllTodoListsVM
                {
                    Lists = _mapper.Map<IList<TodoListDTO>>(entities),
                    PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
                    .Cast<PriorityLevel>()
                    .Select(p => new PriorityLevelDTO { Value = (int)p, Name = p.ToString() })
                    .ToList(),
                };

                _cache.SetItem("todo_lists", result);
                return result;
            }
            else
            {
                return cachedEntities;
            }
        }
    }
}