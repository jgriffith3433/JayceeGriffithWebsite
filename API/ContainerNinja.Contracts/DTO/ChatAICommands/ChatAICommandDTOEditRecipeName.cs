using ContainerNinja.Contracts.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ContainerNinja.Contracts.DTO.ChatAICommands;

[ChatCommandSpecification("edit_recipe_name_and_servings", "Change a recipe's name and servings")]
public record ChatAICommandDTOEditRecipeName : ChatAICommandArgumentsDTO
{
    [Required]
    [Description("Id of the recipe to change the name of")]
    public int RecipeId { get; set; }
    [Required]
    [Description("New name")]
    public string NewName { get; set; }
    [Required]
    [Description("Servings")]
    public int Servings { get; set; }
}
