using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification(new string[] { "search_for_recipes_with_kitchen_products" }, "Returns a list of recipes that a user can use with a list off kitchen products")]
public record ChatAICommandDTOSearchForRecipesWithKitchenProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Kitchen products to search recipes with")]
    public List<ChatAICommandDTOSearchForRecipesWithIngredients_KitchenProduct> KitchenProducts { get; set; }
}

public record ChatAICommandDTOSearchForRecipesWithIngredients_KitchenProduct
{
    [Required]
    [Description("Id of kitchen product")]
    public int KitchenProductId { get; set; }
}
