using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("delete_recipes", "Deletes a list of recipes")]
public record ChatAICommandDTODeleteRecipes : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("List of RecipeIds to delete")]
    public List<int> RecipeIds { get; set; }
}