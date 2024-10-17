using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Services;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;
        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpGet("allUser")]
        public async Task<IActionResult> GetUsers()
        {
            var userDTOs = await _userServices.GetAllUsersAsync();
            return Ok(userDTOs);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserDTO userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var createdUser = await _userServices.CreateUserAsync(userDto);
            return CreatedAtAction(nameof(GetUsers), new { id = createdUser.UserId }, createdUser);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var findUser = await _userServices.GetUserByIdAsync(id);
            if (findUser == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(findUser);
            }
        }

        [HttpDelete("{id}/deleteUser")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var findUser = await _userServices.DeleteUserAsync(id);
            if (findUser == null)
            {
                return NotFound();
            }
            return Ok(findUser);
        }

        [HttpPut("{id}/updateUserById")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO userDTO)
        {
            if (userDTO == null)
            {
                return BadRequest("User data is null.");
            }

            var updatedUser = await _userServices.UpdateUserAsync(id, userDTO);
            if (updatedUser == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(updatedUser);
        }
        [HttpPut("/user/{id}/passwordChange")]
        public async Task<IActionResult> UpdateUserWithPassword(int id, [FromBody] UserDTO userDTO)
        {
            if (userDTO == null)
            {
                return BadRequest("User data is null.");
            }

            var updatedUser = await _userServices.UpdateUserAsyncWithPassword(id, userDTO);
            if (updatedUser == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(updatedUser);
        }
    }
}
