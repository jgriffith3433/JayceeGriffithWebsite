using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("edit_consumed_recipe_ingredient_unit_type", "Changes the unit type for the consumed recipe's ingredient")]
public record ChatAICommandDTOEditCookedRecipeIngredientKitchenUnitType : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the consumed recipe")]
    public int LoggedRecipeId { get; set; }
    [Required]
    [Description("Id of the logged ingredient to change the unit type of")]
    public int LoggedIngredientId { get; set; }
    [Required]
    [Description("New unit type")]
    public KitchenUnitType KitchenUnitType { get; set; }
}
