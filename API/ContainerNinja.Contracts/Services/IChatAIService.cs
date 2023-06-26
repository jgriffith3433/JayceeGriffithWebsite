using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.ViewModels;

namespace ContainerNinja.Contracts.Services
{
    public interface IChatAIService
    {
        Task<ChatMessageVM> GetChatResponse(List<ChatMessageVM> chatMessages, string functionCall, List<ChatRecipeVM> allRecipes, List<ChatKitchenProductVM> allKitchenProducts, List<ChatWalmartProductVM> allWalmartProducts);
        Task<ChatMessageVM> GetNormalChatResponse(List<ChatMessageVM> chatMessages);
        Task<string> GetTextFromSpeech(byte[] speechBytes, string? previousMessage);
    }
}
