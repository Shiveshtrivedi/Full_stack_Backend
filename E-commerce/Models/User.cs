using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace E_commerce.Models
{
    public class PasswordComplexityAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrEmpty(password) || password.Length < 8 ||
             !Regex.IsMatch(password, @"[A-Z]") ||
             !Regex.IsMatch(password, @"[0-9]") ||
             !Regex.IsMatch(password, @"[\W_]"))  
            {
                return new ValidationResult("Password must be at least 8 characters long and contain at least one uppercase letter, one number, and one special character.");
            }

            return ValidationResult.Success;
        }
    }
    public class User
    {
        public int UserId { get; set; }
        [Required]
        public string UserName { get; set; }=   string.Empty;
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [PasswordComplexityAttribute]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[\W_])(?=.*[a-z])(?=.*[0-9]).*$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; } =  string.Empty;
        public bool isAdmin { get; set; } = false;

    }
}
