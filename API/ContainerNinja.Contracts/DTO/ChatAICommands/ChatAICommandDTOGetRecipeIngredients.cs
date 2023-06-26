using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("get_recipe_ingredients", "Gets all of the ingredients for a recipe using RecipeId")]
public record ChatAICommandDTOGetRecipeIngredients : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the recipe to get ingredients of")]
    public int RecipeId { get; set; }
}
