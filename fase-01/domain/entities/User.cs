namespace fase_01.domain.entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user is an administrator.
        /// </summary>
        public bool Admin { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user has been created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the user has validated his email.
        /// </summary>
        public DateTime? ValidatedAt { get; set; }
    }
}