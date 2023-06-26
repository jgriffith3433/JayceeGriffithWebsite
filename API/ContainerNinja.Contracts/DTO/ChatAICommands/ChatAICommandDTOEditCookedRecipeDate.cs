using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

//[ChatCommandSpecification("edit_consumed_recipe_date", "Changes the date for the consumed recipe")]
public record ChatAICommandDTOEditCookedRecipeDate : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the consumed recipe")]
    public int LoggedRecipeId { get; set; }
    [Required]
    [Description("When the recipe was used")]
    public DateTime? When { get; set; }
}
