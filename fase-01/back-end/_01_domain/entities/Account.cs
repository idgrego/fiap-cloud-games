namespace fase_01.domain.entities
{
    public class Account
    {
        public int Id { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public bool Approved { get; set; } = false;
        public int FailedCounter { get; set; } = 0;
        public User User { get; set; } = null!;
    }
}