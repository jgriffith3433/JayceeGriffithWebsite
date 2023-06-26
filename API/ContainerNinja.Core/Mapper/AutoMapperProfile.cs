using AutoMapper;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.ViewModels;
using OpenAI.ObjectModels.RequestModels;

namespace ContainerNinja.Core.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Item, ItemDTO>();

            CreateMap<TodoList, TodoListDTO>();

            CreateMap<TodoItem, TodoItemDTO>()
                .ForMember(d => d.Priority, opt => opt.MapFrom(s => (int)s.Priority));

            CreateMap<WalmartProduct, WalmartProductDTO>()
                .ForMember(d => d.KitchenUnitType, opt => opt.MapFrom(s => (int)s.KitchenUnitType));

            CreateMap<WalmartProduct, WalmartProductDetailsDTO>()
                .ForMember(d => d.KitchenUnitType, opt => opt.MapFrom(s => (int)s.KitchenUnitType));

            CreateMap<WalmartProductDTO, WalmartProductDetailsDTO>();
            CreateMap<WalmartProductDetailsDTO, WalmartProductDTO>();

            CreateMap<CompletedOrder, CompletedOrderDTO>();

            CreateMap<CompletedOrderWalmartProduct, CompletedOrderProductDTO>();

            CreateMap<KitchenProduct, KitchenProductDTO>()
                .ForMember(d => d.ProductId, opt => opt.MapFrom(s => (int)s.WalmartProduct.Id));

            CreateMap<KitchenProduct, KitchenProductDetailsDTO>()
                .ForMember(d => d.ProductId, opt => opt.MapFrom(s => (int)s.WalmartProduct.Id));

            CreateMap<KitchenProductDTO, KitchenProductDetailsDTO>();
            CreateMap<KitchenProductDetailsDTO, KitchenProductDTO>();

            CreateMap<Recipe, RecipeDTO>();

            CreateMap<CalledIngredient, CalledIngredientDetailsDTO>()
                .ForMember(d => d.KitchenUnitType, opt => opt.MapFrom(s => (int)s.KitchenUnitType))
                .ForMember(d => d.KitchenProductId, opt => opt.MapFrom(mapExpression: s => s.KitchenProduct != null ? s.KitchenProduct.Id : -1));

            CreateMap<CalledIngredient, CalledIngredientDTO>()
                .ForMember(d => d.KitchenUnitType, opt => opt.MapFrom(s => (int)s.KitchenUnitType));

            CreateMap<CalledIngredientDTO, CalledIngredientDetailsDTO>();
            CreateMap<CalledIngredientDetailsDTO, CalledIngredientDTO>();

            CreateMap<CookedRecipe, CookedRecipeDTO>()
                .ForMember(d => d.RecipeId, opt => opt.MapFrom(mapExpression: s => s.Recipe != null ? s.Recipe.Id : -1));

            CreateMap<CookedRecipeCalledIngredient, CookedRecipeCalledIngredientDTO>()
                .ForMember(d => d.KitchenUnitType, opt => opt.MapFrom(s => (int)s.KitchenUnitType))
                .ForMember(d => d.CookedRecipeId, opt => opt.MapFrom(s => s.CookedRecipe.Id))
                .ForMember(d => d.KitchenProductId, opt => opt.MapFrom(mapExpression: s => (int?)(s.KitchenProduct != null ? s.KitchenProduct.Id : null)));

            CreateMap<CookedRecipeCalledIngredient, CookedRecipeCalledIngredientDetailsDTO>()
                .ForMember(d => d.KitchenUnitType, opt => opt.MapFrom(s => (int)s.KitchenUnitType))
                .ForMember(d => d.CookedRecipeId, opt => opt.MapFrom(s => s.CookedRecipe.Id))
                .ForMember(d => d.KitchenProductId, opt => opt.MapFrom(mapExpression: s => (int?)(s.KitchenProduct != null ? s.KitchenProduct.Id : null)));

            CreateMap<CookedRecipeCalledIngredientDTO, CookedRecipeCalledIngredientDetailsDTO>();
            CreateMap<CookedRecipeCalledIngredientDetailsDTO, CookedRecipeCalledIngredientDTO>();

            CreateMap<ChatMessage, ChatMessageVM>();
            CreateMap<ChatMessageVM, ChatMessage>();

            CreateMap<Order, OrderDTO>();

            CreateMap<OrderItem, OrderItemDTO>();

        }
    }
}
