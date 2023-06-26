
namespace ContainerNinja.Contracts.DTO
{
    public class TodoListDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }

        public IList<TodoItemDTO> Items { get; set; } = new List<TodoItemDTO>();
    }
}