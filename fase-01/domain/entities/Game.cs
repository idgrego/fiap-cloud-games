using fase_01.domain.enums;

namespace fase_01.domain.entities
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Online { get; set; }
        public bool Multiplayer { get; set; }

        public byte CategoryId { get; set; }
        private GameCategory _category = GameCategory.Unknown;
        public GameCategory Category
        {
            get
            {
                if (_category.Code != CategoryId)
                    _category = GameCategory.FromCode(CategoryId);
                return _category;
            }
            set
            {
                _category = value ?? GameCategory.Unknown;
                CategoryId = _category.Code;
            }
        }

        public string? UrlGame { get; set; }
        public string? UrlVideo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}