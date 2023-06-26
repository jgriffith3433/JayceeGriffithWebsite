using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_product_unit_type", "Changes the product's unit type")]
public record ChatAICommandDTOEditProductKitchenUnitType : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the product to change the unit type of")]
    public int ProductId { get; set; }
    [Required]
    [Description("New unit type")]
    public KitchenUnitType KitchenUnitType { get; set; }
}
