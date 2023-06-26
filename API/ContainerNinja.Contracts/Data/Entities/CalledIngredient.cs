using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.Data.Entities
{
    public class CalledIngredient : AuditableEntity
    {
        public string Name { get; set; }

        public float? Amount { get; set; }

        public bool Verified { get; set; }

        public KitchenUnitType KitchenUnitType { get; set; }

        public virtual KitchenProduct? KitchenProduct { get; set; } = null!;

        public virtual Recipe Recipe { get; set; }

        public virtual IList<CookedRecipeCalledIngredient> CookedRecipeCalledIngredients { get; private set; } = new List<CookedRecipeCalledIngredient>();
    }
}