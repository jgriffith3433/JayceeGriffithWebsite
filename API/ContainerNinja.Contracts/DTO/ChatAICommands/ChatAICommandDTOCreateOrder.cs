using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_new_order", "Creates a new order")]
public record ChatAICommandDTOCreateOrder : ChatAICommandArgumentsDTO
{
    //[Description("When did they get create the new order")]
    //public DateTime? When { get; set; }
}