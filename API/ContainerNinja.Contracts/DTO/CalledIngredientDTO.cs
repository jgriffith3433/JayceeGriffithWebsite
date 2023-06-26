using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Contracts.DTO
{
    public class CalledIngredientDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public KitchenProductDTO KitchenProduct { get; set; }
        public float? Amount { get; set; }
        public int KitchenUnitType { get; set; }
    }
}