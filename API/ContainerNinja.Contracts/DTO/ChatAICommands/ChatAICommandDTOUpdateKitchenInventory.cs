using ContainerNinja.Contracts.Common;
using ContainerNinja.Contracts.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("update_kitchen_product_quantities", "Update existing kitchen product's quantities when taking stock of items in the kitchen")]
public record ChatAICommandDTOUpdateKitchenInventory : ChatAICommandArgumentsDTO
{
    //[Required]
    //[Description("Id of the kitchen inventory to update")]
    //public int KitchenInventoryId { get; set; }

    [Required]
    [Description("List of kitchen products the user is taking stock of")]
    public List<ChatAICommandDTOUpdateKitchenInventory_KitchenProduct> KitchenProducts { get; set; }
}

public record ChatAICommandDTOUpdateKitchenInventory_KitchenProduct
{
    [Description("Id of the kitchen product if it exists in the system")]
    public int? KitchenProductId { get; set; }
    [Required]
    [Description("How many units do they have in stock as a number")]
    public float Quantity { get; set; }
}
