
namespace ContainerNinja.Contracts.Data.Entities
{
    public class OrderItem : AuditableEntity
    {
        public string Name { get; set; }
        public long? WalmartId { get; set; }
        public int Quantity { get; set; }
        public virtual WalmartProduct? WalmartProduct { get; set; }
    }
}