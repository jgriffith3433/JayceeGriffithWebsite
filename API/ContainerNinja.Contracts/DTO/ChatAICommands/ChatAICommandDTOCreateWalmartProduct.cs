using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_walmart_product", "Create a new walmart product to be used for placing orders")]
public record ChatAICommandDTOCreateWalmartProduct : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the product")]
    public string WalmartProductName { get; set; }
}
