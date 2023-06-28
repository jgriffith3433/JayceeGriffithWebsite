using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("get_players", "Get a list of players in a game server")]
public record ChatAICommandDTOGetPlayers : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the server to get players in")]
    public string ServerName { get; set; }
}
