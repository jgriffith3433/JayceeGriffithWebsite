using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.Data.Entities
{
    public class CookedRecipeCalledIngredient : AuditableEntity
    {
        public virtual CookedRecipe CookedRecipe { get; set; }
        public virtual CalledIngredient? CalledIngredient { get; set; }
        public virtual KitchenProduct? KitchenProduct { get; set; }
        public string Name { get; set; }
        public KitchenUnitType KitchenUnitType { get; set; }
        public float? Amount { get; set; }
    }
}