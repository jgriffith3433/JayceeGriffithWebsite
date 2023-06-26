using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("search_kitchen_products", "Gets the ID for a kitchen product by name")]
public record ChatAICommandDTOGetKitchenProductId : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the kitchen product to get an ID for")]
    public string KitchenProductName { get; set; }
}
