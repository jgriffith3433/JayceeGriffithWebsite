
namespace ContainerNinja.Contracts.DTO
{
    public class OrderItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long? WalmartId { get; set; }
        public int Quantity { get; set; }
        public WalmartProductDTO? WalmartProduct { get; set; }
    }
}