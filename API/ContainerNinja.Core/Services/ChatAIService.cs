using ContainerNinja.Contracts.Services;
using Microsoft.AspNetCore.Hosting;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using ContainerNinja.Contracts.ViewModels;
using System.Reflection;
using ContainerNinja.Contracts.Common;
using Newtonsoft.Json;
using System.Dynamic;

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

        public async Task<ChatMessageVM> GetChatResponse(List<ChatMessageVM> chatMessages, string forceFunctionCall)
        {
            var chatCompletionCreateRequest = CreateChatCompletionCreateRequest();
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
        }


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

        private ChatCompletionCreateRequest CreateChatCompletionCreateRequest()
        {
            var chatCompletionCreateRequest = new ChatCompletionCreateRequest
            {
                Messages = GetChatPrompt(),
                Model = "gpt-4",//Models.ChatGpt3_5Turbo0613,
                Functions = _functionSpecifications,
                FunctionCall = "auto",
                Temperature = 0.1f,
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
                Model = "gpt-4",//Models.ChatGpt3_5Turbo0613,
                //Model = Models.ChatGpt3_5Turbo,
                //MaxTokens = 400,
                //FrequencyPenalty = _1,
                //PresencePenalty = _1,
                Temperature = 0.6f
            };
            return chatCompletionCreateRequest;
        }
        
        private List<ChatMessage> GetChatPrompt()
        {
            var chatPromptList = new List<ChatMessage>
            {
                ChatMessage.FromSystem(@"
Your name is aGG
You run game servers and can control characters in a game.
"
, StaticValues.ChatMessageRoles.System),

                ChatMessage.FromAssistant(@"
Hello, how may I help you with your game server? I can perform the following actions:

1. Add a user to a channel or a server.
2. Remove a user from a channel or a server.
3. Get a list of channels in a server.
4. Get a list of game servers.
5. Get a list of object names that can be spawned.
6. Get a list of channels that a user is in.
7. Get a list of users in the game hub.
8. Get a list of users in a server.
9. Spawn objects in a channel.
10. Start a multiplayer server.
11. Stop a multiplayer server.
12. Bomb a user in a channel. 
"
, StaticValues.ChatMessageRoles.Assistant),

                ChatMessage.FromUser(@"When spawning objects use channel 2 which is the sandbox channel and use User 1 as the owner of the object. The game server name is 'New Game Server'."
, StaticValues.ChatMessageRoles.User),

                ChatMessage.FromAssistant(@"
Okay I will use those default values when calling functions on the game server unless otherwise specified.
"
, StaticValues.ChatMessageRoles.Assistant),
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
