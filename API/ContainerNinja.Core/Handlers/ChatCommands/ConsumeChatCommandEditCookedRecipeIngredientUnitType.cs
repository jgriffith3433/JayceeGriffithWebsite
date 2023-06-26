using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "edit_consumed_recipe_ingredient_unit_type" })]
    public class ConsumeChatCommandEditCookedRecipeIngredientKitchenUnitType : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditCookedRecipeIngredientKitchenUnitType>
    {
        public ChatAICommandDTOEditCookedRecipeIngredientKitchenUnitType Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditCookedRecipeIngredientKitchenUnitTypeHandler : IRequestHandler<ConsumeChatCommandEditCookedRecipeIngredientKitchenUnitType, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditCookedRecipeIngredientKitchenUnitTypeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditCookedRecipeIngredientKitchenUnitType model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Set.FirstOrDefault(r => r.Id == model.Command.LoggedRecipeId);

            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find consumed recipe by ID: " + model.Command.LoggedRecipeId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""get_consumed_recipe_id"" }");
            }
            else
            {
                var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Id == model.Command.LoggedIngredientId);
                if (cookedRecipeCalledIngredient == null)
                {
                    var systemResponse = "Could not find logged ingredient by ID: " + model.Command.LoggedIngredientId;
                    throw new ChatAIException(systemResponse);
                }
                else
                {
                    cookedRecipeCalledIngredient.KitchenUnitType = model.Command.KitchenUnitType;
                    _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
                }
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "logged-recipes";
            return "Success";
        }
    }
}