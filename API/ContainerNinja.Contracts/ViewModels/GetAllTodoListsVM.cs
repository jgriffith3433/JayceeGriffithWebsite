using ContainerNinja.Contracts.DTO;

namespace ContainerNinja.Contracts.ViewModels
{
    public record GetAllTodoListsVM
    {
        public IList<PriorityLevelDTO> PriorityLevels { get; set; } = new List<PriorityLevelDTO>();

        public IList<TodoListDTO> Lists { get; set; } = new List<TodoListDTO>();
    }
}