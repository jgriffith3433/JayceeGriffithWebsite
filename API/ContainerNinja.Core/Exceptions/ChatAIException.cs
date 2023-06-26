
namespace ContainerNinja.Core.Exceptions
{
    public class ChatAIException : Exception
    {
        public string ForceFunctionCall { get; private set; }
        public ChatAIException(string error, string forceFunctionCall = "auto") : base(error)
        {
            ForceFunctionCall = forceFunctionCall;
        }
    }
}