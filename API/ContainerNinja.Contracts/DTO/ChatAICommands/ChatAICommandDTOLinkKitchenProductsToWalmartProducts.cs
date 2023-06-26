using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("link_kitchen_products_to_walmart_products", "Links multiple kitchen products to a walmart products by ID")]
public record ChatAICommandDTOLinkKitchenProductsToWalmartProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("List of links to make")]
    public List<ChatAICommandDTOLinkKitchenProductsToWalmartProducts_Link> Links { get; set; }
}

public record ChatAICommandDTOLinkKitchenProductsToWalmartProducts_Link
{
    [Required]
    [Description("ID of the kitchen product")]
    public int KitchenProductId { get; set; }
    [Required]
    [Description("ID of the walmart product")]
    public long WalmartProductId { get; set; }
}