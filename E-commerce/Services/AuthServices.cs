using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCr = BCrypt.Net;

namespace E_commerce.Services
{

    public class AuthServices : IAuthServices
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly MQTTService _mqttService;

        public AuthServices(DataContext context, IMapper mapper, IConfiguration configuration, MQTTService mqttService)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _mqttService = mqttService;
        }
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCr.BCrypt.Verify(password, hashedPassword);
        }


        private string IssueToken(UserDTO userDto)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, userDto.UserName),
            new Claim(ClaimTypes.Email, userDto.Email)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<UserDTO> SignupAsync(UserDTO userDTO)
        {
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDTO.Email);

            if (existingUser != null)
            {
                throw new Exception("User already exists.");
            }

            bool isAdmin = userDTO.Email.EndsWith("@intimetec.com", StringComparison.OrdinalIgnoreCase);

            var passwordHash = PasswordHasher.HashPassword(userDTO.Password);

            var newUser = new User
            {
                UserName = userDTO.UserName,
                Email = userDTO.Email,
                Password = passwordHash,
                isAdmin = isAdmin
            };

            try
            {
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                var userMessage = new 
                {
                    userId = newUser.UserId,
                    userName = newUser.UserName,
                    email = newUser.Email,
                    password = newUser.Password,
                    isAdmin = newUser.isAdmin
                };
                var jsonMessage = JsonConvert.SerializeObject(userMessage);
                await _mqttService.PublishAsync("user/new", jsonMessage);
               

                var userResponse = new UserDTO
                {
                    UserName = newUser.UserName,
                    Email = newUser.Email,
                    isAdmin = newUser.isAdmin
                };
                return userResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during signup.", ex);
            }
        }

        public async Task<UserDTO> LoginAsync(LoginDTO loginDto)
        {
            try
            {
                if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                {
                    throw new ArgumentException("Email and password must be provided.");
                }

                var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null)
                {
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }

                bool isPasswordValid = PasswordHasher.VerifyPassword(loginDto.Password, user.Password);

                if (!isPasswordValid)
                {
                    Console.Error.WriteLine("Invalid credentials: Password does not match.");
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }

                var userDto = _mapper.Map<UserDTO>(user);
                var token = GenerateJwtToken(user);
                userDto.Token = token;

                return userDto;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Login error: {ex.Message}");
                throw;
            }
        }



        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("UserId", user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.isAdmin ? "Admin" : "User")
        };

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
