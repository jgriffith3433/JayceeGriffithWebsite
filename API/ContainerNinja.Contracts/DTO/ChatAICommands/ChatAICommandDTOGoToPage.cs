using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("go_to_page", "Navigate to page.")]
public record ChatAICommandDTOGoToPage : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Page to navigate to.")]
    public string Page { get; set; }
}
