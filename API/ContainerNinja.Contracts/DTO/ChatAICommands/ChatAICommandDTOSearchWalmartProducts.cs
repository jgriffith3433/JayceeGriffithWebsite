using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification(new string[] { "search_walmart_products" }, "Searches for walmart products")]
public record ChatAICommandDTOSearchWalmartProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the walmart product to search for")]
    public string Name { get; set; }
}