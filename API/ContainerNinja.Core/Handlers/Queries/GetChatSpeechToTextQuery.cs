using AutoMapper;
using ContainerNinja.Contracts.Data;
using MediatR;
using ContainerNinja.Contracts.Services;
using Google.Cloud.TextToSpeech.V1;

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetChatTextToSpeechQuery : IRequest<byte[]>
    {
        public string Text { get; set; }
    }

    public class GetChatSpeechToTextQueryHandler : IRequestHandler<GetChatTextToSpeechQuery, byte[]>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private string _GOOGLE_APPLICATION_CREDENTIALS;
        private string _GoogleAuthApiKey;

        public GetChatSpeechToTextQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _chatAIService = chatAIService;
            _GOOGLE_APPLICATION_CREDENTIALS = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            _GoogleAuthApiKey = Environment.GetEnvironmentVariable("GoogleAuthApiKey");
        }

        public async Task<byte[]> Handle(GetChatTextToSpeechQuery request, CancellationToken cancellationToken)
        {
            if (!File.Exists(_GOOGLE_APPLICATION_CREDENTIALS))
            {
                var jsonAuthFile = @"{
  ""type"": ""service_account"",
  ""project_id"": ""weekoffood-388715"",
  ""private_key_id"": ""b34bdad5085be59ced38c7e35df5671a9db4cc06"",
  ""private_key"": """ + _GoogleAuthApiKey + @""",
  ""client_email"": ""weekoffood@weekoffood-388715.iam.gserviceaccount.com"",
  ""client_id"": ""104437502864012212812"",
  ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
  ""token_uri"": ""https://oauth2.googleapis.com/token"",
  ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
  ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/weekoffood%40weekoffood-388715.iam.gserviceaccount.com"",
  ""universe_domain"": ""googleapis.com""
}";
                File.WriteAllText(_GOOGLE_APPLICATION_CREDENTIALS, jsonAuthFile);
            }

            var client = TextToSpeechClient.Create();
            // The input can be provided as text or SSML.
            SynthesisInput input = new SynthesisInput
            {
                Text = request.Text
            };
            // You can specify a particular voice, or ask the server to pick based
            // on specified criteria.
            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = "en-US",
                SsmlGender = SsmlVoiceGender.Female
            };
            // The audio configuration determines the output format and speaking rate.
            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };
            var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);
            byte[] fileBytes;
            using (var output = new MemoryStream())
            {
                // response.AudioContent is a ByteString. This can easily be converted into
                // a byte array or written to a stream.
                response.AudioContent.WriteTo(output);
                fileBytes = output.ToArray();
            }
            return fileBytes;
        }
    }
}