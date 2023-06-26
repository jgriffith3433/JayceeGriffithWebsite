
namespace ContainerNinja.Contracts.Data.Entities
{
    public class Order : AuditableEntity
    {
        public virtual IList<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
    }
}