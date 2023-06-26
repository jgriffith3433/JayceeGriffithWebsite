using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("delete_recipe_ingredient", "Deletes an ingredient in a recipe")]
public record ChatAICommandDTODeleteRecipeIngredient : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the recipe")]
    public int RecipeId { get; set; }
    [Required]
    [Description("Name of the ingredient to delete")]
    public string IngredientName { get; set; }
}
