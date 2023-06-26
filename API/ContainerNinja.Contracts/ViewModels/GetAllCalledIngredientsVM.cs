using ContainerNinja.Contracts.DTO;

namespace ContainerNinja.Contracts.ViewModels
{
    public record GetAllCalledIngredientsVM
    {
        public List<CalledIngredientDTO> CalledIngredients { get; set; }
    }
}