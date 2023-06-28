using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("get_servers", "Get a list of game servers")]
public record ChatAICommandDTOGetServers : ChatAICommandArgumentsDTO
{
}
