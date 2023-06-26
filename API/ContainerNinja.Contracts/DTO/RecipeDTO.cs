using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Contracts.DTO
{
    public class RecipeDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? Serves { get; set; }

        public string? UserImport { get; set; }

        public IList<CalledIngredientDetailsDTO> CalledIngredients { get; set; } = new List<CalledIngredientDetailsDTO>();
    }
}