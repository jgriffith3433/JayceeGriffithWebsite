
namespace ContainerNinja.Contracts.DTO
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public IList<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();
    }
}