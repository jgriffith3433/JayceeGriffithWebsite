using ContainerNinja.Contracts.Walmart;

namespace ContainerNinja.Contracts.Services
{
    public interface IWalmartService
    {
        Task<Search> Search(string query);
        Task<Item> GetItem(string query);
        Task<Item> GetItem(long? id);
        Task<MultipleItems> GetMultipleItems(string[] ids);
    }
}
