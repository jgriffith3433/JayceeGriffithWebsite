using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("get_consumed_recipe_id", "Gets the ID for a consumed recipe by name")]
public record ChatAICommandDTOGetLoggedRecipeId : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Name of the consumed recipe to get an ID for")]
    public string LoggedRecipeName { get; set; }
}
