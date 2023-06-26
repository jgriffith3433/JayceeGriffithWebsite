using ContainerNinja.Contracts.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("delete_consumed_recipe", "Deletes a consumed recipe")]
public record ChatAICommandDTODeleteCookedRecipe : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the recipe log")]
    public int LoggedRecipeId { get; set; }
}
