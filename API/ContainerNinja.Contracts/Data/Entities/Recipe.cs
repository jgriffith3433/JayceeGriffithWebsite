
namespace ContainerNinja.Contracts.Data.Entities
{
    public class Recipe : AuditableEntity
    {
        public string? Name { get; set; }
        public string? UserImport { get; set; }
        public string? Link { get; set; }
        public int? Serves { get; set; }

        public virtual IList<CalledIngredient> CalledIngredients { get; private set; } = new List<CalledIngredient>();
        public virtual IList<CookedRecipe> CookedRecipes { get; private set; } = new List<CookedRecipe>();
    }
}