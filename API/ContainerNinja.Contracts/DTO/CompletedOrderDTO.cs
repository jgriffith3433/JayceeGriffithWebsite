
namespace ContainerNinja.Contracts.DTO
{
    public class CompletedOrderDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? UserImport { get; set; }

        public IList<CompletedOrderProductDTO> CompletedOrderProducts { get; set; } = new List<CompletedOrderProductDTO>();
    }
}