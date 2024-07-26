using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FrameControlKiosk.Models
{
    public class CoordinateCheck
    {
        [Key]
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("coordinateId")]
        public int CoordinateId { get; set; }
        [JsonPropertyName("coordinate")]
        [NotMapped]
        public Coordinate? Coordinate { get; set; }
        [JsonPropertyName("marked")]
        public bool Marked { get; set; }
        [JsonPropertyName("reportMainId")]
        public long ReportMainId { get; set; }
        [JsonPropertyName("reportMain")]
        [NotMapped]
        public ReportMain? ReportMain { get; set; }
        [JsonPropertyName("reportDetailId")]
        public long ReportDetailId { get; set; }
        [JsonPropertyName("reportDetail")]
        [NotMapped]
        public ReportDetail? ReportDetail { get; set; }
        [JsonPropertyName("saved")]
        public bool Saved { get; set; }
    }
}
