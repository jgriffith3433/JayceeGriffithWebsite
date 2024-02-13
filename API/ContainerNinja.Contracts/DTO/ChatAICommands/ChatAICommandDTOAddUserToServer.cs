using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("add_user_to_server", "Add a user to a server")]
public record ChatAICommandDTOAddUserToServer : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Server Name")]
    public string ServerName { get; set; } = "New Game Server";

    [Required]
    [Description("User Name")]
    public string UserName { get; set; } = "User 1";
}
