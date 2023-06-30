using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("spawn_objects_in_channel", "Spawn objects in a channel")]
public record ChatAICommandDTOSpawnObjectsInChannel : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Server Name")]
    public string ServerName { get; set; }

    [Required]
    [Description("Channel Id")]
    public int ChannelId { get; set; }

    [Required]
    [Description("The owner of the object")]
    public string UserName { get; set; }

    [Required]
    [Description("Objects to spawn for user")]
    public List<ChatAICommandDTOSpawnObjectsInChannel_Object> ObjectsToSpawn { get; set; }
}

public record ChatAICommandDTOSpawnObjectsInChannel_Object
{
    [Required]
    [Description("The name of the object")]
    public string Name { get; set; }
}