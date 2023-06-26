using ContainerNinja.Contracts.Common;
using ContainerNinja.Contracts.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_recipe_ingredient_unit_type", "Changes the unit type for the recipe's ingredient")]
public record ChatAICommandDTOEditRecipeIngredientKitchenUnitType : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the recipe")]
    public int RecipeId { get; set; }
    [Required]
    [Description("Id of the ingredient to change the unit type of")]
    public int IngredientId { get; set; }
    [Required]
    [Description("New unit type")]
    public KitchenUnitType KitchenUnitType { get; set; }
}
