namespace ContainerNinja.Contracts.Data.Entities
{
    public class ChatCommand : AuditableEntity
    {
        public string? CommandName { get; set; }
        public string? Error { get; set; }
        public string RawChatAICommand { get; set; }
        public string? CurrentUrl { get; set; }
        public bool UnknownCommand { get; set; }
        public bool ChangedData { get; set; }
        public string? NavigateToPage { get; set; }
        public int ChatConversationId { get; set; }
    }
}