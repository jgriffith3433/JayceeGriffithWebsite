using AutoMapper;
using ContainerNinja.Contracts.ViewModels;
using OpenAI.ObjectModels.RequestModels;

namespace ContainerNinja.Core.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ChatMessage, ChatMessageVM>();
            CreateMap<ChatMessageVM, ChatMessage>();
        }
    }
}
