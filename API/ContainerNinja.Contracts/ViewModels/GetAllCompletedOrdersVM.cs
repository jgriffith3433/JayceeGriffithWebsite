using ContainerNinja.Contracts.DTO;

namespace ContainerNinja.Contracts.ViewModels
{
    public record GetAllCompletedOrdersVM
    {
        public IList<CompletedOrderDTO> CompletedOrders { get; set; } = new List<CompletedOrderDTO>();
    }
}