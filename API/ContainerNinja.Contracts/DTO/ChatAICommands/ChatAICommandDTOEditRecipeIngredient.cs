using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("edit_recipe_ingredient", "Remove an ingredient and add another in a recipe")]
public record ChatAICommandDTOEditRecipeIngredient : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the recipe")]
    public int RecipeId { get; set; }
    [Required]
    [Description("Id of the ingredient to delete")]
    public int IngredientId { get; set; }
    [Required]
    [Description("Name of the new ingredient")]
    public string NewIngredientName { get; set; }
}
