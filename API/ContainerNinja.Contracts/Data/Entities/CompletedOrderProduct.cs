
namespace ContainerNinja.Contracts.Data.Entities
{
    public class CompletedOrderWalmartProduct : AuditableEntity
    {
        public string Name { get; set; }
        public long? WalmartId { get; set; }
        public string? WalmartItemResponse { get; set; }
        public string? WalmartSearchResponse { get; set; }
        public string? WalmartError { get; set; }
        public virtual CompletedOrder CompletedOrder { get; set; }
        public virtual WalmartProduct? WalmartProduct { get; set; }
    }
}