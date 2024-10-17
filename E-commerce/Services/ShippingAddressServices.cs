using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace E_commerce.Services
{
    public class ShippingAddressServices : IShippingAddressServices
    {
        private readonly DataContext _context;

        public ShippingAddressServices(DataContext context)
        {
            _context = context;
        }

        public async Task<List<ShippingAddressDTO>> GetShippingAddressByUserIdAsync(int userId)
        {
            var shippingAddress = await _context.ShippingAddresses.Where(x => x.UserId == userId).ToListAsync();
            if (shippingAddress == null)
            {
                return null;
            }

            return shippingAddress.Select(shippingAddress => new ShippingAddressDTO
            {
                ShippingAddressID = shippingAddress.ShippingAddressID,
                AddressLine1 = shippingAddress.AddressLine1,
                AddressLine2 = shippingAddress.AddressLine2,
                City = shippingAddress.City,
                State = shippingAddress.State,
                ZipCode = shippingAddress.ZipCode,
                Country = shippingAddress.Country
            }).ToList();

        }

        public async Task<ShippingAddressDTO> AddShippingAddressAsync(ShippingAddressDTO shippingAddressDto)
        {
            var shippingAddress = new ShippingAddress
            {
                UserId = shippingAddressDto.UserId,
                AddressLine1 = shippingAddressDto.AddressLine1,
                AddressLine2 = shippingAddressDto.AddressLine2,
                City = shippingAddressDto.City,
                State = shippingAddressDto.State,
                ZipCode = shippingAddressDto.ZipCode,
                Country = shippingAddressDto.Country
            };

            _context.ShippingAddresses.Add(shippingAddress);
            await _context.SaveChangesAsync();

            shippingAddressDto.ShippingAddressID = shippingAddress.ShippingAddressID;
            return shippingAddressDto;
        }

        public async Task<bool> UpdateAddressAsync(int shippingAddressId, ShippingAddressDTO updateAddress)
        {
            var shippingAddress = await _context.ShippingAddresses.FirstOrDefaultAsync(x => x.ShippingAddressID == shippingAddressId);
            if (shippingAddress == null)
                return false;

            shippingAddress.AddressLine1 = updateAddress.AddressLine1;
            if (updateAddress.AddressLine2 != null)
                shippingAddress.AddressLine2 = updateAddress.AddressLine2;
            shippingAddress.City = updateAddress.City;
            shippingAddress.State = updateAddress.State;
            shippingAddress.ZipCode = updateAddress.ZipCode;
            shippingAddress.Country = updateAddress.Country;

            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> DeleteAllAddressAsync(int userId)
        {
            var address = await _context.ShippingAddresses.Where(x => x.UserId == userId).ToListAsync();

            if (address == null)
                return false;

            _context.ShippingAddresses.RemoveRange(address);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAddressById(int shippingAddressId)
        {
            var address = await _context.ShippingAddresses.FirstOrDefaultAsync(x => x.ShippingAddressID == shippingAddressId);
            if (address == null)
                return false;
            _context.ShippingAddresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }




    }
}
