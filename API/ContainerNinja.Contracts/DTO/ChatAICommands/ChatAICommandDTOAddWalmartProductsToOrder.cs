using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("add_walmart_products_to_order", "Add walmart products to order")]
public record ChatAICommandDTOAddWalmartProductsToOrder : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the order to add walmart products to")]
    public int OrderId { get; set; }
    [Required]
    [Description("Walmart Products to order")]
    public List<ChatAICommandDTOAddWalmartProductsToOrder_WalmartProduct> WalmartProducts { get; set; }
}

public record ChatAICommandDTOAddWalmartProductsToOrder_WalmartProduct
{
    [Required]
    [Description("Id of the walmart product")]
    public int WalmartProductId { get; set; }
    [Required]
    [Description("How many should be ordered")]
    public float Quantity { get; set; }
}
