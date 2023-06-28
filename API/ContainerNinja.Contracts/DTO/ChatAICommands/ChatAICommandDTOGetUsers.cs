using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("get_users", "Get a list of users in the game hub")]
public record ChatAICommandDTOGetUsers : ChatAICommandArgumentsDTO
{
}
