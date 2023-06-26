using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("add_kitchen_products_to_order", "Add kitchen products to order")]
public record ChatAICommandDTOAddKitchenProductsToOrder : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the order to add kitchen products to")]
    public int OrderId { get; set; }
    [Required]
    [Description("Kitchen Products to order")]
    public List<ChatAICommandDTOAddKitchenProductsToOrder_KitchenProduct> KitchenProducts { get; set; }
}

public record ChatAICommandDTOAddKitchenProductsToOrder_KitchenProduct
{
    [Required]
    [Description("Id of the kitchen product")]
    public int KitchenProductId { get; set; }
    [Required]
    [Description("How many should be ordered. Convert from the kitchen product unit type to the walmart size to get this number.")]
    public float Quantity { get; set; }
}
