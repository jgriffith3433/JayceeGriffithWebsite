using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("start_server", "Start a multiplayer server")]
public record ChatAICommandDTOStartServer : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("User name of the owner")]
    public string OwnerUserName { get; set; }

    [Required]
    [Description("Name of the server to start")]
    public string ServerName { get; set; }
}
