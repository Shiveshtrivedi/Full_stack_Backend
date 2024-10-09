using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Utils;
using E_commerce.Models;
using Microsoft.EntityFrameworkCore;
using BCr=BCrypt.Net;
using Microsoft.AspNetCore.Http.HttpResults;

namespace E_commerce.Services
{

    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            return BCr.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCr.BCrypt.Verify(password, hashedPassword);
        }
    }

    public class UserServices : IUserServices
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserServices(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            var userDTOs = _mapper.Map<IEnumerable<UserDTO>>(users);
            foreach (var userDto in userDTOs)
            {
                Console.WriteLine($"User Retrieved: Id={userDto.UserId}, UserName={userDto.UserName}, Email={userDto.Email}");
            }
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO> CreateUserAsync(UserDTO userDto)
        {
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

            if(id==' ')
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

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> UpdateUserAsyncWithPassword(int id, UserDTO userDTO)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                Console.WriteLine("User not found");
                return null;
            }

            if (userDTO.Email.EndsWith("@intimetec.com", StringComparison.OrdinalIgnoreCase) &&
                existingUser.Email != userDTO.Email)
            {
                Console.WriteLine("User cannot change email to an Intimetec domain.");
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
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error occurred while updating user: {ex}");
                throw;     
            }

            return _mapper.Map<UserDTO>(existingUser);
        }

        public async Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO updateUserDTO)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                Console.WriteLine("User not found");
                return null;
            }

            if (updateUserDTO.Email != null &&
                updateUserDTO.Email.EndsWith("@intimetec.com", StringComparison.OrdinalIgnoreCase) &&
                existingUser.Email != updateUserDTO.Email)
            {
                Console.WriteLine("User cannot change email to an Intimetec domain.");
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
