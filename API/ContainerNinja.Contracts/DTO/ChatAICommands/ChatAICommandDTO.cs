using System.Dynamic;
using System.Text.Json.Nodes;

namespace ContainerNinja.Contracts.DTO.ChatAICommands
{
    public record ChatAICommandDTO
    {
        public string Name { get; set; }
        public string Arguments { get; set; }
    }
}
