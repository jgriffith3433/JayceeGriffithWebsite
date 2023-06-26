using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "search_walmart_products_for_kitchen_product" }, "Searches for walmart products using a list of KitchenProductId")]
public record ChatAICommandDTOSearchWalmartProductsForKitchenProduct : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("IDs of a Kitchen Products to search for")]
    public int[] KitchenProductIds { get; set; }
}