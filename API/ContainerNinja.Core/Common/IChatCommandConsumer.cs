using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;

namespace ContainerNinja.Core.Common
{
    public interface IChatCommandConsumer<T> where T : ChatAICommandArgumentsDTO
    {
        public T Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }
}
