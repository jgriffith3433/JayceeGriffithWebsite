using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("search_kitchen_products", "Search the kitchen inventory for kitchen product names returns KitchenProductId and ProductName fields")]
public record ChatAICommandDTOSearchKitchenProducts : ChatAICommandArgumentsDTO
{
    //[Required]
    //[Description("Id of the kitchen inventory")]
    //public int KitchenInventoryId { get; set; }
    [Required]
    [Description("List of names to search for")]
    public List<string> ListOfNames { get; set; }
}
