using E_commerce.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs
{
    public class ShippingAddressDTO
    {
        public int ShippingAddressID { get; set; }

        public int UserId { get; set; }

        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; } = string.Empty;
        public int PhoneNumber { get; set; }
        [Required]
        public string State { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        [Required]
        public int ZipCode { get; set; }
        [Required]
        public string Country { get; set; } = string.Empty;
    }
}
