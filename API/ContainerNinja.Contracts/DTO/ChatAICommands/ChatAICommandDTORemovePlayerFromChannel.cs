using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("remove_player_from_channel", "Remove player from channel")]
public record ChatAICommandDTORemovePlayerFromChannel : ChatAICommandArgumentsDTO
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
