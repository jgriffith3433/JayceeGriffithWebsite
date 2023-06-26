using ContainerNinja.Contracts.Common;
using ContainerNinja.Contracts.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_new_recipe_ingredients", "Create new ingredients for a recipe")]
public record ChatAICommandDTOUpdateRecipe : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the recipe")]
    public int RecipeId { get; set; }
    [Description("List of step by step instructions")]
    public List<string> Instructions { get; set; }
    [Required]
    [Description("List of kitchen products")]
    public List<ChatAICommandDTOUpdateRecipe_KitchenProduct>? KitchenProducts { get; set; }
}

public record ChatAICommandDTOUpdateRecipe_KitchenProduct
{
    [Required]
    [Description("Id of the kitchen product")]
    public int KitchenProductId { get; set; }
    [Required]
    [Description("How many units does the recipe call for as a number")]
    public float? Quantity { get; set; }
    [Description("Kitchen unit type for the kitchen product")]
    public KitchenUnitType? UnitType { get; set; }
}
