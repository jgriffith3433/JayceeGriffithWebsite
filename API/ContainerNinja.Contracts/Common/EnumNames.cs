
namespace ContainerNinja.Contracts.Common
{
    public class EnumNames : Attribute
    {
        public EnumNames(params string[] args)
        {
            Names = args;
        }

        public string[] Names { get; set; }
    }
}