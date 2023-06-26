using ContainerNinja.Contracts.DTO;

namespace ContainerNinja.Contracts.ViewModels
{
    public record GetAllCookedRecipesVM
    {
        public List<CookedRecipeDTO> CookedRecipes { get; set; }
        public List<KitchenUnitTypeDTO> KitchenUnitTypes { get; set; }
        public List<RecipesOptionDTO> RecipesOptions { get; set; } = new List<RecipesOptionDTO>();
    }
}