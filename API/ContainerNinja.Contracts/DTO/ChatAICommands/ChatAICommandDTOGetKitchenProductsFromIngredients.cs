using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("get_kitchen_products_from_ingredients", "Gets kitchen products from a list of IngredientsId")]
public record ChatAICommandDTOGetKitchenProductsFromIngredients : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("IDs of the ingredients")]
    public int[] IngredientIds { get; set; }
}
