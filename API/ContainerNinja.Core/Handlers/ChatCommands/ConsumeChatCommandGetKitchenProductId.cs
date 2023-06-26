using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using ContainerNinja.Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ContainerNinja.Contracts.Walmart;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    //[ChatCommandModel(new[] { "search_kitchen_products" })]
    public class ConsumeChatCommandGetKitchenProductId : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetKitchenProductId>
    {
        public ChatAICommandDTOGetKitchenProductId Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetKitchenProductIdHandler : IRequestHandler<ConsumeChatCommandGetKitchenProductId, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGetKitchenProductIdHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandGetKitchenProductId model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<KitchenProduct>();
            var searchTerms = string.Join(' ', model.Command.KitchenProductName.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
                if (searchTerm[searchTerm.Length - 1] == 's')
                {
                    predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm.Substring(0, searchTerm.Length - 1)));
                }
            }

            var query = _repository.KitchenProducts.Set.AsExpandable().Where(predicate).ToList();

            KitchenProduct kitchenProduct;
            if (query.Count == 0)
            {
                var systemResponse = "Could not find kitchen product by name: " + model.Command.KitchenProductName;
                throw new ChatAIException(systemResponse);
            }
            else if (query.Count == 1)
            {
                if (query[0].Name.ToLower() == model.Command.KitchenProductName.ToLower())
                {
                    //exact match
                    kitchenProduct = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Could not find kitchen product by name '" + model.Command.KitchenProductName + "'. Did you mean: '" + query[0].Name + "' with ID: " + query[0].Id + "?";
                    throw new ChatAIException(systemResponse);
                }
            }
            else
            {
                var exactMatch = query.FirstOrDefault(ps => ps.Name.ToLower() == model.Command.KitchenProductName.ToLower());

                if (exactMatch != null)
                {
                    //check if there are no "too similar" matches
                    //for instance: "trail mix" could be "raisin free trail mix" or "trail mix snack"
                    var tooSimilarMatch = query.FirstOrDefault(ps => ps != exactMatch && ps.Name.ToLower().Contains(model.Command.KitchenProductName.ToLower()));
                    if (tooSimilarMatch != null)
                    {
                        exactMatch = null;
                    }
                }

                if (exactMatch != null)
                {
                    //exact match
                    kitchenProduct = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Multiple records found: " + string.Join(", ", query.Select(r => r.Name));
                    throw new ChatAIException(systemResponse);
                }
            }
            if (kitchenProduct == null)
            {
                var systemResponse = "Could not find kitchen product by name '" + model.Command.KitchenProductName + "'.";
                throw new ChatAIException(systemResponse);
            }
            var kitchenProductObject = new JObject();
            kitchenProductObject["KitchenProductId"] = kitchenProduct.Id;
            kitchenProductObject["KitchenProductName"] = kitchenProduct.Name;
            model.Response.ForceFunctionCall = "auto";
            model.Response.NavigateToPage = "kitchen-products";
            return JsonConvert.SerializeObject(kitchenProductObject, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}