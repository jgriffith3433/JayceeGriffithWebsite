using System.Text.Json;
using System.Text.Json.Nodes;

namespace ContainerNinja.Contracts.ViewModels
{
    public record ChatMessageVM
    {
        public string From { get; set; }
        public string? Name { get; set; }
        public string? Content { get; set; }
        public string? FunctionCall { get; set; }
        public string? To { get; set; }
        public bool Received { get; set; }
    }
}