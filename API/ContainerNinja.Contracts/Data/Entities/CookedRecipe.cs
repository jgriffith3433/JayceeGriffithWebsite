
namespace ContainerNinja.Contracts.Data.Entities
{
    public class CookedRecipe : AuditableEntity
    {
        public virtual Recipe Recipe { get; set; }

        public virtual IList<CookedRecipeCalledIngredient> CookedRecipeCalledIngredients { get; private set; } = new List<CookedRecipeCalledIngredient>();
    }
}