using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Services;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/shippingAddress")]
    public class ShippingAddressController : ControllerBase
    {
        private readonly IShippingAddressServices _shippingAddressServices;

        public ShippingAddressController(IShippingAddressServices shippingAddressServices)
        {
            _shippingAddressServices = shippingAddressServices;
        }

        [HttpGet("{userId}/getAddressByUserId")]
        public async Task<ActionResult<IEnumerable<ShippingAddressDTO>>> GetShippingAddresses(int userId)
        {
            var addresses = await _shippingAddressServices.GetShippingAddressByUserIdAsync(userId);

            if (addresses == null)
                return BadRequest();

            return Ok(addresses);
        }


        [HttpPost("addAddress")]
        public async Task<ActionResult<ShippingAddressDTO>> AddShippingAddress([FromBody] ShippingAddressDTO shippingAddressDto)
        {
            var result = await _shippingAddressServices.AddShippingAddressAsync(shippingAddressDto);
            return Ok(CreatedAtAction(nameof(GetShippingAddresses), new { id = result.ShippingAddressID }, result));
        }

        [HttpPut("{shippingAddressId}/updateAddress")]
        public async Task<IActionResult> UpdateShippingAdress(int shippingAddressId, [FromBody] ShippingAddressDTO shippingAddressDTO)
        {
            var result = await _shippingAddressServices.UpdateAddressAsync(shippingAddressId, shippingAddressDTO);

            if (result)
            {
                return Ok("updated successfully");
            }
            return NotFound("not found ");
        }

        [HttpDelete("user/{userId}/deleteAddressByUserId")]
        public async Task<IActionResult> DeleteAllAddress(int userId)
        {
            var result = await _shippingAddressServices.DeleteAllAddressAsync(userId);

            if (result)
            {
                return Ok("all address deleted");
            }

            return BadRequest();
        }

        [HttpDelete("{shippingAddressId}/deleteAddressByShippingAddressId")]
        public async Task<IActionResult> DeleteAddressByShippingId(int shippingAddressId)
        {
            var result = await _shippingAddressServices.DeleteAddressById(shippingAddressId);

            if (result)
            {
                return Ok("addrress deletd successfully");
            }

            return NotFound("Given address not found");
        }



    }
}
