
namespace ContainerNinja.Contracts.DTO
{
    public class CompletedOrderProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long? WalmartId { get; set; }
        public string? WalmartItemResponse { get; set; }
        public string? WalmartSearchResponse { get; set; }
        public string? WalmartError { get; set; }
        public int CompletedOrderId { get; set; }

        public WalmartProductDTO? Product { get; set; }
    }
}