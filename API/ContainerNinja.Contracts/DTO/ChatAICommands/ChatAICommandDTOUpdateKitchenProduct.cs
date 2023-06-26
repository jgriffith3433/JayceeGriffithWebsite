//using ContainerNinja.Contracts.Common;
//using ContainerNinja.Contracts.Enum;
//using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;

//namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("update_kitchen_products", "Update existing kitchen products when taking stock of items in the kitchen")]
//public record ChatAICommandDTOUpdateKitchenProducts : ChatAICommandArgumentsDTO
//{
//    [Required]
//    [Description("List of products the user is taking stock of")]
//    public List<ChatAICommandDTOUpdateKitchenProducts_KitchenProduct> KitchenProducts { get; set; }
//}

//public record ChatAICommandDTOUpdateKitchenProducts_KitchenProduct
//{
//    [Description("Id of the kitchen product if it exists in the system")]
//    public int? KitchenProductId { get; set; }
//    [Required]
//    [Description("How many units do they have in stock")]
//    public float Amount { get; set; }
//    [Required]
//    [Description("Kitchen unit type for the kitchen product")]
//    public KitchenUnitType KitchenUnitType { get; set; }
//}
