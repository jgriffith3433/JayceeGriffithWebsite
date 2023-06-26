
namespace ContainerNinja.Contracts.ViewModels
{
    public record GetChatResponseVM
    {
        public List<ChatMessageVM> ChatMessages { get; set; }
        public int ChatConversationId { get; set; }
        public string CurrentUrl { get; set; }
        public string ForceFunctionCall { get; set; }
    }
}