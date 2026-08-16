using System.ComponentModel.DataAnnotations.Schema;

namespace fase_01.domain.entities
{
    public abstract class PhotoBase
    {
        public int Id { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public byte[] Image { get; set; } = Array.Empty<byte>();
        public byte[]? Thumbnail { get; set; }
    }
}