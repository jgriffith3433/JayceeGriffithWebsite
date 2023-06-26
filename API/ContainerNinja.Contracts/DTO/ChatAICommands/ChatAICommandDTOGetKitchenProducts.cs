using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("get_kitchen_inventory_products", "Get all products in the kitchen inventory using the KitchenInventoryId")]
public record ChatAICommandDTOGetKitchenInventory : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the kitchen inventory")]
    public int KitchenInventoryId { get; set; } 
}
