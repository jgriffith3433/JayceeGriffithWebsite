using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("bomb_user_in_channel", "Spawn a rocket in a channel and fire it at a user")]
public record ChatAICommandDTOBombUserInChannel : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Server Name")]
    public string ServerName { get; set; } = "New Game Server";

    [Required]
    [Description("The user to bomb")]
    public string UserToBomb { get; set; } = "User 1";

}