using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("remove_user_from_channel", "Remove a user from a channel")]
public record ChatAICommandDTORemoveUserFromChannel : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Server Name")]
    public string ServerName { get; set; }

    [Required]
    [Description("Channel Id")]
    public int ChannelId { get; set; }

    [Required]
    [Description("Name of the user to remove from the channel")]
    public string UserName { get; set; }
}
