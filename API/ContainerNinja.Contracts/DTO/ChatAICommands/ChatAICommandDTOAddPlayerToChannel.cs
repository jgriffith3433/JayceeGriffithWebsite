using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("add_player_to_channel", "Add a player to a channel")]
public record ChatAICommandDTOAddPlayerToChannel : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Server Name")]
    public string ServerName { get; set; }

    [Required]
    [Description("Channel Id")]
    public int ChannelId { get; set; }

    [Required]
    [Description("Name of the player to add to the channel")]
    public string PlayerName { get; set; }
}
