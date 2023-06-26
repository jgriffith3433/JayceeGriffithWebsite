using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Contracts.DTO
{
    public class CalledIngredientDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public KitchenProductDTO KitchenProduct { get; set; }
        public float? Amount { get; set; }
        public int KitchenUnitType { get; set; }
        public int KitchenProductId { get; set; }
        //public KitchenUnitType KitchenUnitType { get; set; }

        public IList<KitchenProductDTO> KitchenProductSearchItems { get; set; } = new List<KitchenProductDTO>();
    }
}