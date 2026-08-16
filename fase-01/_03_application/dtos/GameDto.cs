using System.ComponentModel.DataAnnotations;

namespace fase_01.application.dtos
{
    public class GameDto
    {
        public int Id { get; set; }

        [Display(Name = "Game")]
        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(255, ErrorMessage = "The field {0} must be a maximum length of {1}")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Manufacturer")]
        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(255, ErrorMessage = "The field {0} must be a maximum length of {1}")]
        public string Manufacturer { get; set; } = string.Empty;

        [Display(Name = "Released At")]
        public DateOnly? ReleasedAt { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Online")]
        public bool Online { get; set; }

        [Display(Name = "Multiplayer")]
        public bool Multiplayer { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Category")]
        public byte CategoryId { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Game Page")]
        [Url(ErrorMessage = "Invalid URL. Please inform a valid URL.")]
        [MaxLength(255, ErrorMessage = "The field {0} must be a maximum length of {1}")]
        public string? UrlGame { get; set; }

        [Display(Name = "Trailer/Video URL")]
        [Url(ErrorMessage = "Invalid URL. Please inform a valid URL.")]
        [MaxLength(255, ErrorMessage = "The field {0} must be a maximum length of {1}")]
        public string? UrlVideo { get; set; }

        [Display(Name = "Cover / Image")]
        public IFormFile? Photo { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
    }
}