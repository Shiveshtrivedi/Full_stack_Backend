using E_commerce.DTOs;
using E_commerce.Models;
using Razorpay.Api;

namespace E_commerce.Utils
{
    public interface IUserServices
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> GetUserByIdAsync(int id);
        Task<UserDTO> CreateUserAsync(UserDTO userDTO);
        Task<UserDTO> DeleteUserAsync(int id);
        Task<UserDTO> UpdateUserAsyncWithPassword(int id, UserDTO userDTO);
        Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO updateUserDTO);

    }

    public interface IProductServices
    {
        Task<IEnumerable<E_commerce.Models.Product>> GetAllProductsAsync();
        Task<E_commerce.Models.Product> CreateProductAsync(ProductDTO productDTO, int userId);
        Task<List<E_commerce.Models.Product>> AddProductsAsync(List<ProductDTO> productDtos);
        Task<E_commerce.Models.Product> GetProductByIdAsync(int id);
        Task<IEnumerable<E_commerce.Models.Product>> GetProductByUserIdAsync(int userId);
        Task<E_commerce.Models.Product> UpdateProductAsync(ProductDTO productDto, int id);
        Task<E_commerce.Models.Product> DeleteProductAsync(int id);
    }

    public interface ICartServices
    {
        Task<IEnumerable<CartDTO>> GetAllProductsFromCartAsync(int userId);
        Task<CartDTO[]> AddToCartAsync(int userId, int productId, int quantity);
        Task<List<CartDTO>> UpdateCartByUserAsync(int userId, List<UpdateCartItemDTO> updateCartItemsDto);
        Task<bool> ClearCartItemByUserAsync(int userId);
    }

    public interface IOrderServices
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<OrderDTO> GetOrderByIdAsync(int orderId);
        Task<OrderDTO> PlaceOrderAsync(CreateOrderDTO orderDTO);
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
        Task<bool> DeleteReviewAsync(int reviewId, int userId);
    }

    public interface IAuthServices
    {
        Task<UserDTO> SignupAsync(UserDTO userDTO);
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
        Task<List<RevenueDTO>> CalculateTotalRevenueAsync();
        Task<RevenueDTO> GetRevenueByDateAsync(DateTime date);
        Task<RevenueByDateDTO> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDTO>> GetAllInventoriesAsync();
        Task<InventoryDTO> GetInventoryByProductIdAsync(int productId);
        Task<List<ProductSaleDTO>> UpdateStockAsync(UpdateStockDTO updateStockDto);
        Task<List<ProductSaleDTO>> CreateInventoryAsync(int productId);
        Task<bool> AdminIncreaseStockAsync(AdminUpdateStockDTO updateStockDto);
    }

    public interface IAdminHistoryService
    {
        Task<IEnumerable<HistoryDTO>> GetAllHistoryAsync();
        Task<List<HistoryDTO>> GetHistoryByUserIdAsync(int userId);
        Task<bool> DeleteHistoryAsync(int historyId);
        Task<bool> ClearHistoryAsync(int userId);

    }

    public interface IRazorpayService
    {
        Task<Razorpay.Api.Order> CreateOrderAsync(decimal amount, string currency = "INR", string receipt = "order_rcptid_11");
    }

}

