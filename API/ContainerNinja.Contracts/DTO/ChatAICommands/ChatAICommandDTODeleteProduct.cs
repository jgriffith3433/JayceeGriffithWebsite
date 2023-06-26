using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("delete_kitchen_products", "Deletes a list of kitchen products")]
public record ChatAICommandDTODeleteKitchenProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("List of the kitchen product to delete")]
    public List<ChatAICommandDTODeleteKitchenProduct_Product> KitchenProductsToDelete { get; set; }
}

public record ChatAICommandDTODeleteKitchenProduct_Product
{
    [Required]
    [Description("Id of the kitchen product")]
    public int KitchenProductId { get; set; }
}