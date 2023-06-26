using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_kitchen_products" })]
    public class ConsumeChatCommandSearchKitchenProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchKitchenProducts>
    {
        public ChatAICommandDTOSearchKitchenProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSearchKitchenProductHandler : IRequestHandler<ConsumeChatCommandSearchKitchenProducts, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSearchKitchenProductHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchKitchenProducts model, CancellationToken cancellationToken)
        {
            var listOfResults = new List<List<KitchenProduct>>();
            if (model.Command.ListOfNames == null || model.Command.ListOfNames.Count == 0)
            {
                model.Command.ListOfNames = new List<string> { "*" };
            }
            foreach (var name in model.Command.ListOfNames)
            {
                var predicate = PredicateBuilder.New<KitchenProduct>();
                if (string.IsNullOrEmpty(name) || name == "*" || name == "stock" || name == "all")
                {
                    predicate = predicate.Or(r => true);
                }
                else
                {
                    predicate = predicate.Or(r => r.Name.ToLower() == name.ToLower());
                    if (name[name.Length - 1] == 's')
                    {
                        predicate = predicate.Or(r => r.Name.ToLower() == name.Substring(0, name.Length - 1));
                    }
                    //var searchTerms = string.Join(' ', name.ToLower().Split('-')).Split(' ');
                    //foreach (var searchTerm in searchTerms)
                    //{
                    //    predicate = predicate.Or(r => r.Name.ToLower().Contains(searchTerm));
                    //    if (searchTerm[searchTerm.Length - 1] == 's')
                    //    {
                    //        predicate = predicate.Or(r => r.Name.ToLower().Contains(searchTerm.Substring(0, searchTerm.Length - 1)));
                    //    }
                    //}
                }
                listOfResults.Add(_repository.KitchenProducts.Set.AsExpandable().Where(predicate).ToList());
            }
            var allSearchResultsObject = new JObject();
            for (var i = 0; i < model.Command.ListOfNames.Count; i++)
            {
                var results = listOfResults[i];
                var searchResultsObject = new JObject
                {
                    { "Message", $"{results.Count} records found for {model.Command.ListOfNames[i]}" }
                };
                if (results.Count > 0)
                {
                    var foundArray = new JArray();
                    foreach (var kitchenProduct in results)
                    {
                        var foundObject = new JObject();
                        foundObject["KitchenProductId"] = kitchenProduct.Id;
                        foundObject["KitchenProductName"] = kitchenProduct.Name;
                        foundObject["Quantity"] = kitchenProduct.Amount;
                        foundArray.Add(foundObject);
                    }
                    searchResultsObject.Add("Results", foundArray);
                }
                allSearchResultsObject.Add(model.Command.ListOfNames[i], searchResultsObject);
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();

            model.Response.NavigateToPage = "kitchen-products";
            return JsonConvert.SerializeObject(allSearchResultsObject, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}