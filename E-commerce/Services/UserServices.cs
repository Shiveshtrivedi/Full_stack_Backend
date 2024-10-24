using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Utils;
using E_commerce.Models;
using Microsoft.EntityFrameworkCore;
using BCr = BCrypt.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace E_commerce.Services
{

    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            if(string.IsNullOrWhiteSpace(password) || password.Contains(" "))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            return BCr.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            {
                throw new ArgumentException("Password and hashed password cannot be null or empty.");
            }

            return BCr.BCrypt.Verify(password, hashedPassword);
        }
    }

    public class UserServices : IUserServices
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly MQTTService _mqttService;
        public UserServices(DataContext context, IMapper mapper, MQTTService mqttService)
        {
            _context = context;
            _mapper = mapper;
            _mqttService = mqttService;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var userDTOs = _mapper.Map<IEnumerable<UserDTO>>(users);

                return userDTOs;
            }
            catch(Exception ex)
            {
                throw new Exception("Error retrieving users", ex);
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName) || userName.Length < 3 || userName.Length > 15)
            {
                return false;
            }

            return true;
        }

        public async Task<UserDTO> CreateUserAsync(UserDTO userDto)
        {
            var context = new ValidationContext(userDto);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(userDto, context, validationResults, true);

            if (!IsValidEmail(userDto.Email))
            {
                validationResults.Add(new ValidationResult("Please provide a valid email address."));
                isValid = false;
            }

            if (!IsValidUserName(userDto.UserName))
            {
                validationResults.Add(new ValidationResult("Username must be between 3 and 15 characters, and cannot be empty or contain only spaces."));
                isValid = false;
            }


            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    Console.WriteLine(validationResult.ErrorMessage);      
                }

                throw new Exception("Model validation failed");
            }


            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);

            if (existingUser != null)
            {
                throw new Exception("User already exists.");
            }

            bool isAdmin = userDto.Email.EndsWith("@intimetec.com", StringComparison.OrdinalIgnoreCase);
            userDto.isAdmin = isAdmin;
            var passwordHash = PasswordHasher.HashPassword(userDto.Password);
            var user = _mapper.Map<User>(userDto);
            user.Password = passwordHash;
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var jsonMessage = JsonConvert.SerializeObject(user);
                await _mqttService.PublishAsync("user/new", jsonMessage);
                
                
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the user.", ex);
            }

            return _mapper.Map<UserDTO>(user);

        }
        public async Task<UserDTO> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (id == ' ')
            {
                return null;
            }

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var userMessage = new
            {
                userId = user.UserId,
                userName = user.UserName,
                email = user.Email,
                isAdmin = user.isAdmin
            };

            var jsonMessage = JsonConvert.SerializeObject(userMessage);
            await _mqttService.PublishAsync("user/delete", jsonMessage);

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> UpdateUserAsyncWithPassword(int id, UserDTO userDTO)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return null;
            }

            if (userDTO.Email.EndsWith("@intimetec.com", StringComparison.OrdinalIgnoreCase) &&
                existingUser.Email != userDTO.Email)
            {
                return null;
            }

            existingUser.UserName = userDTO.UserName;

            if (existingUser.Email != userDTO.Email)
            {
                existingUser.Email = userDTO.Email;
            }

            existingUser.isAdmin = userDTO.isAdmin;

            if (!string.IsNullOrEmpty(userDTO.Password))
            {
                existingUser.Password = PasswordHasher.HashPassword(userDTO.Password);
            }

            try
            {
                await _context.SaveChangesAsync();
                var userMessage = new
                {
                    userId = existingUser.UserId,
                    userName = existingUser.UserName,
                    email = existingUser.Email,
                    isAdmin = existingUser.isAdmin
                };

                var jsonMessage = JsonConvert.SerializeObject(userMessage);
                await _mqttService.PublishAsync("user/update", jsonMessage);

            }
            catch (DbUpdateException ex)
            {
                throw;
            }

            return _mapper.Map<UserDTO>(existingUser);
        }

        public async Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO updateUserDTO)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return null;
            }

            if (updateUserDTO.Email != null &&
                updateUserDTO.Email.EndsWith("@intimetec.com", StringComparison.OrdinalIgnoreCase) &&
                existingUser.Email != updateUserDTO.Email)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(updateUserDTO.UserName))
            {
                existingUser.UserName = updateUserDTO.UserName;
            }

            if (!string.IsNullOrEmpty(updateUserDTO.Email) && existingUser.Email != updateUserDTO.Email)
            {
                existingUser.Email = updateUserDTO.Email;
            }

            existingUser.isAdmin = updateUserDTO.IsAdmin;

            try
            {
                await _context.SaveChangesAsync();
                var userMessage = new
                {
                    userId = existingUser.UserId,
                    userName = existingUser.UserName,
                    email = existingUser.Email,
                    isAdmin = existingUser.isAdmin
                };

                var jsonMessage = JsonConvert.SerializeObject(userMessage);
                await _mqttService.PublishAsync("user/update", jsonMessage);

            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error occurred while updating user: {ex}");
                throw;
            }

            return _mapper.Map<UserDTO>(existingUser);
        }


    }
}
