using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace E_commerce.Models
{
    public class ComplexityAttribute : ValidationAttribute
    {
        public bool ValidatePassword { get; set; }
        public bool ValidateEmail { get; set; }
        public bool ValidateUserName { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var field = value as string;

            if (ValidatePassword)
            {
                if (string.IsNullOrEmpty(field) || field.Length < 8 ||
                    !Regex.IsMatch(field, @"[A-Z]") ||
                    !Regex.IsMatch(field, @"[0-9]") ||
                    !Regex.IsMatch(field, @"[a-z]") ||
                    !Regex.IsMatch(field, @"[\W_]"))
                {
                    return new ValidationResult("Password must be at least 8 characters long and contain one uppercase letter, one number, and one special character.");
                }
            }

            if (ValidateEmail)
            {
                if (string.IsNullOrEmpty(field) || !Regex.IsMatch(field, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    return new ValidationResult("Please provide a valid email address.");
                }
            }

            if (ValidateUserName)
            {
                if (string.IsNullOrWhiteSpace(field))
                {
                    return new ValidationResult("Username cannot be empty or contain only spaces.");
                }

                if (field.Length < 3 || field.Length > 15)
                {
                    return new ValidationResult("Username must be between 3 and 15 characters.");
                }
            }

            return ValidationResult.Success;
        }
    }

        public class User
        {
            public int UserId { get; set; }
            [Required]
            [ComplexityAttribute(ValidateUserName = true)]
            public string UserName { get; set; } = string.Empty;
            [Required]
            [EmailAddress(ErrorMessage = "Invalid email format.")]
            [ComplexityAttribute(ValidateEmail = true)]
            public string Email { get; set; } = string.Empty;
            [Required]
            [ComplexityAttribute(ValidatePassword = true)]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*[\W_])(?=.*[a-z])(?=.*[0-9]).*$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
            public string Password { get; set; } = string.Empty;
            public bool isAdmin { get; set; } = false;

        }
    }

