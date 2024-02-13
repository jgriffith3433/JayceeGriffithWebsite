using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("spawn_objects_in_channel", "Spawn objects in a channel")]
public record ChatAICommandDTOSpawnObjectsInChannel : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Server Name")]
    public string ServerName { get; set; } = "New Game Server";

    [Required]
    [Description("Channel Id")]
    public int ChannelId { get; set; } = 2;

    [Required]
    [Description("The owner of the object")]
    public string UserName { get; set; } = "User 1";

    [Required]
    [Description("Objects to spawn for user")]
    public List<ChatAICommandDTOSpawnObjectsInChannel_Object> ObjectsToSpawn { get; set; }
}

public record ChatAICommandDTOSpawnObjectsInChannel_Object
{
    [Required]
    [Description("The name of the object")]
    public string Name { get; set; }

    [Required]
    [Description("How many to spawn")]
    public int Quantity { get; set; } = 1;
}