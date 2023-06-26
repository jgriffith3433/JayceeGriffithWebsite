
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.Data.Entities
{
    public class KitchenProduct : AuditableEntity
    {
        public string Name { get; set; }
        public float Amount { get; set; }
        public KitchenUnitType KitchenUnitType { get; set; }
        public virtual WalmartProduct? WalmartProduct { get; set; } = null;
    }
}