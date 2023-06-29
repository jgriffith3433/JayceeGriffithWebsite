using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("get_users_channels", "Get a list of channels that a user is in")]
public record ChatAICommandDTOGetUsersChannels : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Server Name")]
    public string ServerName { get; set; }

    [Required]
    [Description("User Name")]
    public string UserName { get; set; }
}
