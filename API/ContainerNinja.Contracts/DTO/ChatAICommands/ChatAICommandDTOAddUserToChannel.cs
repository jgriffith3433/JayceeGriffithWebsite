using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("add_user_to_channel", "Add a user to a channel with option to stay in current channels")]
public record ChatAICommandDTOAddUserToChannel : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Server Name")]
    public string ServerName { get; set; } = "New Game Server";

    [Required]
    [Description("Channel Id")]
    public int ChannelId { get; set; }

    [Required]
    [Description("Name of the user to add to the channel")]
    public string UserName { get; set; } = "User 1";

    [Required]
    [Description("Add user to another channel and keep them in the ones they are already in")]
    public bool StayInChannels { get; set; }
}
