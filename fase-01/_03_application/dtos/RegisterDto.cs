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
        [MinLength(6, ErrorMessage = "The field {0} must be a minimum length of {1}")]
        public string? Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "The field {0} is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Photo")]
        public IFormFile? Photo { get; set; }
    }
}