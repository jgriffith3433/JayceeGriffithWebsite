using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("link_recipe_ingredient_to_kitchen_product", "Links a recipe's ingredient to a kitchen product by ID")]
public record ChatAICommandDTOLinkRecipeIngredientToKitchenProduct : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("ID of the recipe")]
    public int RecipeId { get; set; }
    [Required]
    [Description("ID of the ingredient")]
    public int IngredientId { get; set; }
    [Required]
    [Description("ID of the kitchen product")]
    public long KitchenProductId { get; set; }
}
