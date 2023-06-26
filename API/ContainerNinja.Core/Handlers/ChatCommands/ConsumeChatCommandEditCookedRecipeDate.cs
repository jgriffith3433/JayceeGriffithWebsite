using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "edit_consumed_recipe_date" })]
    public class ConsumeChatCommandEditCookedRecipeDate : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditCookedRecipeDate>
    {
        public ChatAICommandDTOEditCookedRecipeDate Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditCookedRecipeDateHandler : IRequestHandler<ConsumeChatCommandEditCookedRecipeDate, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditCookedRecipeDateHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditCookedRecipeDate model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Set.FirstOrDefault(r => r.Id == model.Command.LoggedRecipeId);

            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find consumed recipe by ID: " + model.Command.LoggedRecipeId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""get_consumed_recipe_id"" }");
            }

            //TODO:Add in date
            //cookedRecipe.Name = model.Command.NewName;
            //_repository.CookedRecipes.Update(cookedRecipe);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "logged-recipes";
            return "Success";
        }
    }
}