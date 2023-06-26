using ContainerNinja.Contracts.DTO;

namespace ContainerNinja.Contracts.ViewModels
{
    public record GetAllKitchenProductsVM
    {
        public List<KitchenProductDTO> KitchenProducts { get; set; }
        public List<KitchenUnitTypeDTO> KitchenUnitTypes { get; set; }
    }
}