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

        /// <summary>
        /// Permite receber a imagem pelo JSON em formato base64 para realização do upsert da foto do usuário
        /// </summary>
        [Display(Name = "Photo")]
        public string? PhotoBase64 { get; set; }

        /// <summary>
        /// Informa quando o usuário foi criado no sistema (autoregistro)
        /// </summary>
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Informa quando o usuário foi validado.
        /// Ao realizar o autoregistro o usuário receberá um e-mail com um link.
        /// Ao clicar no link irá comprovar/validar o endereço de e-mail do usuário.
        /// </summary>
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