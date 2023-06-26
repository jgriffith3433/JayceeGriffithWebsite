using ContainerNinja.Contracts.Common;
using ContainerNinja.Contracts.Enum;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("add_kitchen_products_to_inventory", "Add kitchen products to inventory")]
public record ChatAICommandDTOCreateKitchenProducts : ChatAICommandArgumentsDTO
{
    //[Required]
    //[Description("Id of the kitchen inventory record")]
    //public int KitchenInventoryId { get; set; }

    [Required]
    [Description("List of kitchen products the user is taking stock of")]
    public List<ChatAICommandDTOCreateKitchenProducts_KitchenProduct> KitchenProducts { get; set; }
}

public record ChatAICommandDTOCreateKitchenProducts_KitchenProduct
{
    [Required]
    [Description("Name of the kitchen product")]
    public string KitchenProductName { get; set; }
    [Description("The number of kitchen units they have. Example: 3 packages")]
    public float Quantity { get; set; }
    public float Amount { get => Quantity; set => Quantity = value; }
    public float Units { get => Quantity; set => Quantity = value; }
    [Required(ErrorMessage = "KitchenUnitType is a required field. Please refer to the KitchenUnitType Values List")]
    [Description("Unit type for the kitchen product")]
    public KitchenUnitType KitchenUnitType { get; set; }
}