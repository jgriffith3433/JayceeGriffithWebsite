
namespace ContainerNinja.Contracts.ViewModels
{
    public record ChatResponseVM
    {
        public int ChatConversationId { get; set; }
        public bool CreateNewChat { get; set; }
        public bool Error { get; set; }
        public bool Dirty { get; set; }
        public bool UnknownCommand { get; set; }
        public string NavigateToPage { get; set; }
        public string ForceFunctionCall { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
    }
}