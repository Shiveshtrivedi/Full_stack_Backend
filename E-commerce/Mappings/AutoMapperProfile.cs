using AutoMapper;
using E_commerce.DTOs;
using E_commerce.Models;

namespace E_commerce.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<Cart, CartDTO>().ReverseMap();
            CreateMap<Cart, AddCartItemDTO>().ReverseMap();
            CreateMap<CartItem, CartItemDTO>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.Image))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ReverseMap();

            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.TransctionId, opt => opt.MapFrom(src => src.TransctionId));


            CreateMap<OrderDetail, OrderDetailDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName));

            CreateMap<WishList, WishlistReadDto>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));

            CreateMap<WishlistCreateDto, WishList>();

            CreateMap<Sale, SalesDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Order.OrderDetails.FirstOrDefault().Product.ProductName));

            CreateMap<CreateSaleDTO, Sale>();

            CreateMap<RevenueDTO, Sale>().ReverseMap();

            CreateMap<Product, InventoryDTO>()
                .ForMember(dest => dest.StockAvailable, opt => opt.MapFrom(src => src.Stock))
                .ForMember(dest => dest.StockSold, opt => opt.Ignore());

            CreateMap<Inventory, InventoryDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.StockAvailable, opt => opt.MapFrom(src => src.StockAvailable))
                .ForMember(dest => dest.StockSold, opt => opt.MapFrom(src => src.StockSold));

            CreateMap<Product, ProductDTO>().ReverseMap();
            CreateMap<AdminHistory, HistoryDTO>().ReverseMap();
        }
    }
}

