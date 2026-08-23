namespace fase_01.application.dtos
{
    public class JwtSettingsDto
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationInMinutes { get; set; } = 60;
        public int RefreshTokenExpirationInDays { get; set; } = 30;
    }
}