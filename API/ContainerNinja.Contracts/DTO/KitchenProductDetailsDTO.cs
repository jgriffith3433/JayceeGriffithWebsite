
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.DTO
{
    public class KitchenProductDetailsDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public float? Amount { get; set; }

        public KitchenUnitType KitchenUnitType { get; set; }

        public int? ProductId { get; set; }

        public WalmartProductDTO Product { get; set; }

        public IList<WalmartProductDTO> ProductSearchItems { get; set; } = new List<WalmartProductDTO>();
    }
}