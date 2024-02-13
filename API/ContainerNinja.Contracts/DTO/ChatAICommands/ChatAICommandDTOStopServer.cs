using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("stop_server", "Stop the multiplayer server")]
public record ChatAICommandDTOStopServer : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the server to stop")]
    public string ServerName { get; set; } = "New Game Server";
}
