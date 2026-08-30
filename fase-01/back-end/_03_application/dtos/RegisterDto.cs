using System.ComponentModel.DataAnnotations;

namespace fase_01.application.dtos
{
    public class RegisterDto
    {
        [Display(Name = "Fullname")]
        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(255, ErrorMessage = "The field {0} must be a maximum length of {1}")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Nickname")]
        [MaxLength(50, ErrorMessage = "The field {0} must be a maximum length of {1}")]
        public string? NickName { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "The field {0} is required")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [MaxLength(255, ErrorMessage = "The field {0} must be a maximum length of {1}")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password")]
        [Required(ErrorMessage = "The field {0} is required")]
        [DataType(DataType.Password)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "The field {0} must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        /// <summary>
        /// User password. Must meet complexity requirements.
        /// </summary>
        /// <remarks>
        /// Regex breakdown (^((?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$)):
        /// - ^ : Start of text.
        /// - (?=.*[a-z]) : At least 1 lowercase letter.
        /// - (?=.*[A-Z]) : At least 1 uppercase letter.
        /// - (?=.*\d) : At least 1 number.
        /// - (?=.*[^\da-zA-Z]) : At least 1 special character.
        /// - .{8,}$ : Minimum 8 characters in length.
        /// </remarks>
        public string? Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "The field {0} is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        /// <summary>
        /// Permite receber a imagem pelo JSON em formato base64
        /// </summary>
        [Display(Name = "Photo")]
        public string? PhotoBase64 { get; set; }
    }
}