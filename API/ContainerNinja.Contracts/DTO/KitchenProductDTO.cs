
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.DTO
{
    public class KitchenProductDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public float? Amount { get; set; }

        public KitchenUnitType KitchenUnitType { get; set; }

        public int? ProductId { get; set; }

        public WalmartProductDTO WalmartProduct { get; set; }
    }
}