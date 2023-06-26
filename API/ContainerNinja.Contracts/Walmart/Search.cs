
namespace ContainerNinja.Contracts.Walmart
{
    public class Search
    {
        public string query { get; set; }
        public string sort { get; set; }
        public string responseGroup { get; set; }
        public long totalResults { get; set; }
        public long start { get; set; }
        public long numItems { get; set; }
        public List<Item> items { get; set; }
    }
}
