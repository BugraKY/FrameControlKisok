using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FrameControlKiosk.Models
{
    public class Coordinate
    {
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("x")]
        public double X { get; set; }
        [JsonPropertyName("y")]
        public double Y { get; set; }
        [JsonPropertyName("imageType")]
        public int ImageType { get; set; }
        [JsonPropertyName("markSize")]
        public int MarkSize { get; set; }
    }
}
