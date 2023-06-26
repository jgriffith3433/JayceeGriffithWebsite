
namespace ContainerNinja.Core.Common
{
    public class ChatCommandModel : Attribute
    {
        public ChatCommandModel(params string[] commandNames)
        {
            CommandNames = commandNames;
        }

        public string[] CommandNames { get; set; }
    }
}
