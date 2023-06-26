using ContainerNinja.Contracts.Common;
using ContainerNinja.Contracts.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("create_new_kitchen_products", "Creates kitchen products and returns the KitchenProductId of each one")]
public record ChatAICommandDTOCreateNewKitchenProducts : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("List of kitchen products the user is taking stock of")]
    public List<ChatAICommandDTOCreateKitchenProducts_KitchenProduct> KitchenProducts { get; set; }
}

public record ChatAICommandDTOCreateNewKitchenProducts_KitchenProduct
{
    [Required]
    [Description("Name of the kitchen product")]
    public string KitchenProductName { get; set; }
    [Description("The number of kitchen units they have. Example: 3 packages")]
    public float Quantity { get; set; }
    //public float Amount { get => Quantity; set => Quantity = value; }
    //public float Units { get => Quantity; set => Quantity = value; }
    [Required(ErrorMessage = "KitchenUnitType is a required field. Please refer to the KitchenUnitType Values List")]
    [Description("Unit type for the kitchen product")]
    public KitchenUnitType KitchenUnitType { get; set; }
}