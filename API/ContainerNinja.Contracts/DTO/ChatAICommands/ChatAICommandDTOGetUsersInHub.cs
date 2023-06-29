using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("get_users_in_hub", "Get a list of users in the game hub")]
public record ChatAICommandDTOGetUsersInHub : ChatAICommandArgumentsDTO
{
}
