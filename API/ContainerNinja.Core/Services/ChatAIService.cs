using ContainerNinja.Contracts.Services;
using Microsoft.AspNetCore.Hosting;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using System.Reflection;
using ContainerNinja.Contracts.Common;
using Newtonsoft.Json;
using System.Dynamic;
using Microsoft.IdentityModel.Tokens;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data.Entities;
using System.Collections.Generic;

namespace ContainerNinja.Core.Services
{
    public class ChatAIService : IChatAIService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IOpenAIService _openAIService;
        private static IList<FunctionDefinition> _functionSpecifications = GetChatCommandSpecifications();

        public ChatAIService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _openAIService = new OpenAIService(new OpenAiOptions()
            {
                Organization = Environment.GetEnvironmentVariable("OpenAIServiceOrganization"),
                ApiKey = Environment.GetEnvironmentVariable("OpenAIServiceApiKey"),
            });
        }

        public static IList<FunctionDefinition> GetChatCommandSpecifications()
        {
            try
            {
                var chatFunctions = new List<FunctionDefinition>();
                foreach (var ccsType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.GetCustomAttribute<ChatCommandSpecification>() != null))
                {
                    var ccs = ccsType.GetCustomAttribute<ChatCommandSpecification>();

                    foreach (var name in ccs.Names)
                    {
                        chatFunctions.Add(new FunctionDefinition
                        {
                            Name = name,
                            Description = ccs.Description,
                            Parameters = ccs.GetFunctionParametersFromType(ccsType),
                        });
                    }
                }
                return chatFunctions;
            }
            catch (Exception ex)
            {
                //weird error
                /*Could not load type 'Castle.Proxies.CookedRecipeCalledIngredientProxy' from assembly 'DynamicProxyGenAssembly2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'.
                 at System.Reflection.RuntimeModule.GetTypes(RuntimeModule module)
                */
                return new List<FunctionDefinition>();
            }
        }

        public async Task<ChatMessageVM> GetChatResponse(List<ChatMessageVM> chatMessages, string forceFunctionCall, List<ChatRecipeVM> allRecipes, List<ChatKitchenProductVM> allKitchenProducts, List<ChatWalmartProductVM> allWalmartProducts)
        {
            var chatCompletionCreateRequest = CreateChatCompletionCreateRequest(allRecipes, allKitchenProducts, allWalmartProducts);
            if (!string.IsNullOrEmpty(forceFunctionCall) && forceFunctionCall.Contains("{"))
            {
                chatCompletionCreateRequest.FunctionCall = JsonConvert.DeserializeObject<ExpandoObject>(forceFunctionCall);
            }
            else
            {
                chatCompletionCreateRequest.FunctionCall = forceFunctionCall;
            }
            chatMessages.ForEach(cm => chatCompletionCreateRequest.Messages.Add(new ChatMessage(cm.From, cm.Content, cm.Name)));
            var completionResult = await _openAIService.ChatCompletion.CreateCompletion(chatCompletionCreateRequest);
            if (completionResult.Error != null)
            {
                throw new Exception(completionResult.Error.Message);
            }
            else
            {
                var chatResponse = completionResult.Choices.First().Message;

                return new ChatMessageVM
                {
                    Content = chatResponse.Content,
                    From = chatResponse.Role,
                    To = GetToFromChatResponse(chatResponse),
                    Name = chatResponse.Name,
                    FunctionCall = FunctionCallToJson(chatResponse.FunctionCall),
                };
            }
        }

        private string GetToFromChatResponse(ChatMessage chatMessage)
        {
            if (chatMessage.FunctionCall != null)
            {
                return "function";
            }
            else
            {
                return StaticValues.ChatMessageRoles.User;
            }
            //if (IsString(chatMessage.FunctionCall))
            //{
            //    return "function";
            //}
            //else
            //{
            //    if (IsDefined(chatMessage.FunctionCall))
            //    {
            //        return "function";
            //    }
            //    else
            //    {
            //        return StaticValues.ChatMessageRoles.User;
            //    }
            //}
        }

        //public bool IsDefined(JsonObject? jsonElement)
        //{
        //    return jsonElement.HasValue && jsonElement.Value.ValueKind != JsonValueKind.Null && jsonElement.Value.ValueKind != JsonValueKind.Undefined;
        //}

        //public bool IsString(JsonObject? jObject)
        //{
        //    return jObject.HasValue && jObject.Value.ValueKind == JsonValueKind.String;
        //}

        public async Task<ChatMessageVM> GetNormalChatResponse(List<ChatMessageVM> chatMessages)
        {
            var chatCompletionCreateRequest = CreateNormalChatCompletionCreateRequest();
            chatMessages.ForEach(cm => chatCompletionCreateRequest.Messages.Add(new ChatMessage(cm.From, cm.Content, cm.Name)));
            var completionResult = await _openAIService.ChatCompletion.CreateCompletion(chatCompletionCreateRequest);

            if (completionResult.Error != null)
            {
                throw new Exception(completionResult.Error.Message);
            }
            else
            {
                var chatResponse = completionResult.Choices.First().Message;

                return new ChatMessageVM
                {
                    Content = chatResponse.Content,
                    From = chatResponse.Role,
                    To = GetToFromChatResponse(chatResponse),
                    Name = chatResponse.Name,
                    FunctionCall = FunctionCallToJson(chatResponse.FunctionCall),
                };
            }
        }

        protected static FunctionParameters? JsonToFunctionParameters(string? jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<FunctionParameters>(jsonString, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
        }

        protected static string? FunctionCallToJson(FunctionCall? functionCall)
        {
            if (functionCall == null)
            {
                return null;
            }
            return JsonConvert.SerializeObject(functionCall, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        private ChatCompletionCreateRequest CreateChatCompletionCreateRequest(List<ChatRecipeVM> allRecipes, List<ChatKitchenProductVM> allKitchenProducts, List<ChatWalmartProductVM> allWalmartProducts)
        {
            var chatCompletionCreateRequest = new ChatCompletionCreateRequest
            {
                Messages = GetChatPrompt(allRecipes, allKitchenProducts, allWalmartProducts),
                Model = "gpt-3.5-turbo-16k-0613",//Models.ChatGpt3_5Turbo0613,
                Functions = _functionSpecifications,
                FunctionCall = "auto",
                Temperature = 0.2f,
                //MaxTokens = 400,
                //FrequencyPenalty = _1,
                //PresencePenalty = _1,
                //TopP = 0.f,
            };
            return chatCompletionCreateRequest;
        }

        private ChatCompletionCreateRequest CreateNormalChatCompletionCreateRequest()
        {
            var chatCompletionCreateRequest = new ChatCompletionCreateRequest
            {
                Messages = GetNormalChatPrompt(),
                Model = Models.ChatGpt3_5Turbo,
                //MaxTokens = 400,
                //FrequencyPenalty = _1,
                //PresencePenalty = _1,
                Temperature = 0.6f
            };
            return chatCompletionCreateRequest;
        }

        private string[] GetPages()
        {
            return new string[]
            {
                "home",
                "todo",
                "portfolio",
                "kitchen products",
                "walmart products",
                "orders",
                "completed orders",
                "recipes",
                "consumed recipes",
                "called ingredients",
                "api",
            };
        }
        /*
ChatMessage.FromSystem(@"You are a kitchen assistant, your name is Kitchy. In general, if you are unsure of a value, you should stop and ask the user for clarification.", StaticValues.ChatMessageRoles.System),
ChatMessage.FromSystem(@"This is the flow of updating the kitchen inventory..", StaticValues.ChatMessageRoles.System),
ChatMessage.FromUser(@"I'm going to tell you what kitchen products I have in my kitchen and how much I have of each one. Before you update the system, can you verify with me first?", StaticValues.ChatMessageRoles.User),
ChatMessage.FromAssistant(@"Of course! Please go ahead and tell me the kitchen products you have in your kitchen and how much of each one you have. I will search for them in the system based on the provided names and then verify with you before updating the system.", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"[User provides the kitchen products and quantities without specifying KitchenProductIds].", StaticValues.ChatMessageRoles.System),
ChatMessage.FromSystem(@"[Assistant calls the function to search for the kitchen products based on the provided names and retrieves the corresponding KitchenProductIds]", StaticValues.ChatMessageRoles.System),
ChatMessage.FromAssistant(@"Got it! I have found the KitchenProductIds for the kitchen products you mentioned. Now, can I update your inventory with the specified quantities?.", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"[User indicates to go ahead and update the system].", StaticValues.ChatMessageRoles.User),
ChatMessage.FromSystem(@"[Assistant calls the function to update the inventory for each kitchen product using the retrieved KitchenProductIds and using the units and unit types that the user specified].", StaticValues.ChatMessageRoles.System),
ChatMessage.FromAssistant(@"Great! I have updated your inventory with the specified quantities of each kitchen product. Is there anything else you would like to add or update?", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"[User provides additional kitchen products or quantities without specifying KitchenProductIds].", StaticValues.ChatMessageRoles.System),
ChatMessage.FromSystem(@"[Assistant calls the function to search for the additional kitchen products based on the provided names and retrieves the corresponding KitchenProductIds]", StaticValues.ChatMessageRoles.System),
ChatMessage.FromAssistant(@"Thank you for providing the kitchen products and quantities. I will update your inventory once you confirm the quantities and products. Can I update your inventory with the specified quantities?", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"[User indicates to go ahead and update the system].", StaticValues.ChatMessageRoles.System),
ChatMessage.FromSystem(@"[Assistant calls the function to update the inventory for each additional kitchen product using the retrieved KitchenProductIds and using the units and unit types that the user specified]", StaticValues.ChatMessageRoles.System),
ChatMessage.FromAssistant(@"Great! I have updated your inventory with the specified quantities of each additional kitchen product. Is there anything else you would like to add or update?", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"[User indicates no further updates].", StaticValues.ChatMessageRoles.System),
ChatMessage.FromAssistant(@"Alright if you need any further assistance, feel free to let me know!", StaticValues.ChatMessageRoles.Assistant),
ChatMessage.FromSystem(@"End of updating kitchen inventory flow.", StaticValues.ChatMessageRoles.System),

*/

        /*
User: ""Start kitchen inventory flow""
Assistant: ""Okay how can I help you manage your kitchen inventory?
Use the following loop until the user has finished working on their inventory
[BEGIN kitchen_inventory_loop]
1) [User tells you about the KitchenProductAmount and KitchenUnitType of a kitchen product in their kitchen]
2) [If you do not have all of the KitchenProductId values then call the ""search_kitchen_products"" function]
3) [If you found the KitchenProductId values from searching then ask the user if you can update that record]
4) [If you didn't find the KitchenProductId values then call the ""add_kitchen_products"" function]
5) [If you have the KitchenProductId values from either ""search_kitchen_products"" or ""add_kitchen_products"" and you have enough information to fill out Amount and KitchenUnitType ask the user if you can call the ""update_kitchen_products"" function]
6) [If the user gives you permission to call ""update_kitchen_products"" then call it and respond back to the user what the results are and then ask if there is anything else you can help them with]
7) [Repeat this until the user says they are done working managing their kitchen inventory]
[END kitchen_inventory_loop]

*/
        /*
         
This is the flow of updating the kitchen inventory.
User: ""Start kitchen inventory flow""
Assistant: ""Okay, how can I help you manage your kitchen inventory?""
Use the following loop until you are finished managing your inventory:
[BEGIN kitchen_inventory_loop]
1) User tells you about the KitchenProductAmount and KitchenUnitType of a kitchen product in their kitchen.
2) If you do not have all of the KitchenProductId values, call the ""search_kitchen_products"" function.
3) If you found the KitchenProductId values from searching, ask the user if you can update that record.
4) If you didn't find the KitchenProductId values, call the ""add_kitchen_products"" function.
5) If you have the KitchenProductId values from either ""search_kitchen_products"" or ""add_kitchen_products"" and you have enough information to fill out Amount and KitchenUnitType, ask the user for permission to call the ""update_kitchen_products"" function.
6) If the user gives you permission, call the ""update_kitchen_products"" function and respond back to the user with the results. Then ask if there is anything else you can help them with.
7) Repeat this loop until the user says they are done managing their kitchen inventory.
[END kitchen_inventory_loop]

         */
        /*
Your name is Kitchy, and you are an extremely logical and rigid inventory management ai.
You help the user manage food items in their kitchen.
You are very clear about what action you are about to perform. You call functions only when you have all of the neccessary information.
You update the inventory only when a user tells you to.
When creating, searching or updating the kitchen inventory respond back with the KitchenProductId of each item and ask if you can update the system.
You are not allowed to make up arguments when calling functions.
You must verify with the user about the arguments in a function before calling them.
You must NEVER say you called a function without doing so prior*/

        private List<ChatMessage> GetChatPrompt(List<ChatRecipeVM> allRecipes, List<ChatKitchenProductVM> allKitchenProducts, List<ChatWalmartProductVM> allWalmartProducts)
        {
            var chatPromptList = new List<ChatMessage>
{//You help the user log meals, create/modify/delete logged meals, recipes, and ingredients. You help the user in using the website.
                ChatMessage.FromSystem($@"
Your name is Kitchy and you are a helpful kitchen assistant. You update the system with user information.
This is not a demonstration.
This is not a simulation and this affects real-world systems and databases through the functions provided.

Here is a list of available unit types:
{JsonConvert.SerializeObject(Enum.GetValues(typeof(KitchenUnitType)).Cast<KitchenUnitType>().Select(p => p.ToString()), Formatting.Indented) }
Here is a list of available recipes:
{JsonConvert.SerializeObject(allRecipes, Formatting.Indented) }
Here is a list of available kitchen products:
{JsonConvert.SerializeObject(allKitchenProducts, Formatting.Indented) }
Here is a list of available walmart products:
{JsonConvert.SerializeObject(allWalmartProducts, Formatting.Indented) }
Here is a list of available pages:
{JsonConvert.SerializeObject(GetPages(), Formatting.Indented) }

Here's how the flow of creating a recipe goes:
[User names recipe]
[Assistant asks what ingredient's go in it]
[User tells ingredients]
[Assistant updates recipe]


If a user says ""x y of z"" then the quantity is x, the unit type is y and the name is z
"
, StaticValues.ChatMessageRoles.System),
    };
            return chatPromptList;
        }

        private List<ChatMessage> GetNormalChatPrompt()
        {
            var chatPromptList = new List<ChatMessage>
{
ChatMessage.FromSystem("You are a conversationalist. Have fun talking with the user.", StaticValues.ChatMessageRoles.System),
};
            return chatPromptList;
        }

        public async Task<string> GetTextFromSpeech(byte[] speechBytes, string? previousMessage)
        {
            var response = await _openAIService.Audio.CreateTranscription(new AudioCreateTranscriptionRequest
            {
                FileName = "blob.wav",
                File = speechBytes,
                Model = Models.WhisperV1,
                ResponseFormat = StaticValues.AudioStatics.ResponseFormat.VerboseJson,
                Prompt = previousMessage,
                Temperature = 0.2f,
                Language = "en",
            });
            if (!response.Successful)
            {
                if (response.Error == null)
                {
                    throw new Exception("Unknown Error");
                }
                else
                {
                    throw new Exception(JsonConvert.SerializeObject(response.Error));
                }
            }
            return string.Join("\n", response.Text);
        }
    }
}
