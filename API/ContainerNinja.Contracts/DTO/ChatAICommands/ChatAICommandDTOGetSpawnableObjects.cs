using ContainerNinja.Contracts.Common;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("get_spawnable_objects", "Get a list of object names that can be spawned")]
public record ChatAICommandDTOGetSpawnableObjects : ChatAICommandArgumentsDTO
{
}
