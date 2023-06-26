using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.DTO
{
    public class CookedRecipeCalledIngredientDTO
    {
        public int Id { get; set; }
        public int CookedRecipeId { get; set; }
        public CalledIngredientDTO? CalledIngredient { get; set; }
        public KitchenProductDTO? KitchenProduct { get; set; }
        public int? KitchenProductId { get; set; }
        public string Name { get; set; }
        public KitchenUnitType KitchenUnitType { get; set; }
        public float? Amount { get; set; }
    }
}