using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Contracts.DTO
{
    public class CookedRecipeDTO
    {
        public int Id { get; set; }

        public RecipeDTO Recipe { get; set; }

        public int RecipeId { get; set; }

        public IList<CookedRecipeCalledIngredientDTO> CookedRecipeCalledIngredients { get; set; }
    }
}