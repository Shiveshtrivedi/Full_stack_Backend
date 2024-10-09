using E_commerce.DTOs;
using E_commerce.Models;

namespace E_commerce.Utils
{
    public interface IUserServices
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> GetUserByIdAsync(int id);
        Task<UserDTO> CreateUserAsync(UserDTO userDTO);
        Task<UserDTO> DeleteUserAsync(int id);
        Task<UserDTO> UpdateUserAsyncWithPassword(int id,UserDTO userDTO);
        Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO updateUserDTO);

    }

    public interface IProductServices
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> CreateProductAsync(ProductDTO productDTO,int userId);
        Task<List<Product>> AddProductsAsync(List<ProductDTO> productDtos);
        Task<Product> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetProductByUserIdAsync(int userId);
        Task<Product> UpdateProductAsync(ProductDTO productDto,int id);
        Task<Product> DeleteProductAsync(int id);
    }

    public interface ICartServices
    {
        Task<IEnumerable<CartDTO>> GetAllProductsFromCartAsync(int userId);
        Task<bool> AddProductInCartAsync(int userId, AddCartItemDTO addCartItemDto);
        //Task<CartDTO> UpdateCartByUserAsync(int userId, UpdateCartItemDTO updatedItemDto);
        Task<List<CartDTO>> UpdateCartByUserAsync(int userId, List<UpdateCartItemDTO> updateCartItemsDto);
        Task<bool> ClearCartItemByUserAsync(int userId);
    }

    public interface IOrderServices
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<OrderDTO> GetOrderByIdAsync(int orderId);
        //Task<OrderDTO?> PlaceOrderAsync(CreateOrderDTO orderDTO);
        Task<int?> PlaceOrderAsync(CreateOrderDTO orderDTO);

        Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<OrderDTO>> UpdateOrderAsync(int orderId, OrderUpdateDTO orderUpdateDTO);

    }

    public interface IShippingAddressServices
    {
        Task<List<ShippingAddressDTO>> GetShippingAddressByUserIdAsync(int userId);
        Task<ShippingAddressDTO> AddShippingAddressAsync(ShippingAddressDTO shippingAddressDto);
        Task<bool> UpdateAddressAsync(int shippingAddressId, ShippingAddressDTO updateAddress);
        Task<bool> DeleteAllAddressAsync(int userId);
        Task<bool> DeleteAddressById(int shippingAddressId);
    }

    public interface IReviewServices
    {
        Task<List<ReviewDTO>> GetAllReviewsAsync();
        Task<List<ReviewDTO>> GetReviewsByProductAsync(int productId);
        Task<ReviewDTO> AddReviewAsync(ReviewDTO reviewDto);
        Task<bool> DeleteReviewAsync(int reviewId,int userId);
    }

    public interface IAuthServices
    {
        Task<bool> SignupAsync(UserDTO userDTO);
        //Task<UserDTO> LoginAsync(LoginDTO loginDto);
        //Task<(UserDTO, string token)> LoginAsync(LoginDTO loginDto);
        Task<UserDTO> LoginAsync(LoginDTO loginDto);

    }

    public interface IWishListServices
    {
        Task<List<WishlistReadDto>> GetUserWishlist(int userId);

        Task<WishlistReadDto> AddProductToWishList(int userId, int productId);
        Task<bool> DeleteProductFromWishList(int userId, int productId);
    }

    public interface ISalesService
    {
        Task<IEnumerable<SalesDTO>> GetAllSalesAsync();
        Task<SalesDTO> AddSaleAsync(CreateSaleDTO createSaleDTO);
        Task<IEnumerable<SalesDTO>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<SalesDTO> GetSaleByOrderIdAsync(int orderId);
        Task<SalesComparisonResultDTO> CompareSalesAsync(SalesComparisonDTO currentPeriod, SalesComparisonDTO previousPeriod);

    }
    public interface IRevenueService
    {
        Task<decimal> CalculateTotalRevenueAsync();
        Task<RevenueDTO> GetRevenueByDateAsync(DateTime date);
        Task<RevenueByDateDTO> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDTO>> GetAllInventoriesAsync();
        Task<InventoryDTO> GetInventoryByProductIdAsync(int productId);
        //Task<bool> UpdateStockAsync(UpdateStockDTO updateStockDto);
        Task<List<ProductSaleDTO>> UpdateStockAsync(UpdateStockDTO updateStockDto);
        Task<bool> CreateInventoryAsync(int productId);
        Task<bool> AdminIncreaseStockAsync(AdminUpdateStockDTO updateStockDto);
    }

    public interface IAdminHistoryService
    {
        Task<IEnumerable<HistoryDTO>> GetAllHistoryAsync();
        Task<List<HistoryDTO>> GetHistoryByUserIdAsync(int userId);
        Task<bool> DeleteHistoryAsync(int historyId);
        Task<bool> ClearHistoryAsync(int userId);

    }
}

