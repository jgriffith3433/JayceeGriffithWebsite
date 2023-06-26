using ContainerNinja.Contracts.DTO;

namespace ContainerNinja.Contracts.ViewModels
{
    public record GetAllOrdersVM
    {
        public IList<OrderDTO> Orders { get; set; } = new List<OrderDTO>();
    }
}