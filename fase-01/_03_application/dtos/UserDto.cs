using System.ComponentModel.DataAnnotations;

namespace fase_01.application.dtos
{
    public class UserDto : IValidatableObject
    {
        public int Id { get; set; }

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

        [Display(Name = "Admin")]
        public bool Admin { get; set; }

        [Display(Name = "Photo")]
        public IFormFile? Photo { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Validated At")]
        public DateTime? ValidatedAt { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ValidatedAt.HasValue && ValidatedAt.Value < CreatedAt)
            {
                yield return new ValidationResult(
                    "The field Validated At must be greater than or equal to Created At.",
                     [nameof(ValidatedAt)]
                );
            }
        }
    }
}