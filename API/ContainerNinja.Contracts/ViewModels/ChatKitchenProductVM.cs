
namespace ContainerNinja.Contracts.ViewModels
{
    public record ChatKitchenProductVM
    {
        public int KitchenProductId { get; set; }
        public string KitchenProductName { get; set; }
        public float Quantity { get; set; }
    }
}