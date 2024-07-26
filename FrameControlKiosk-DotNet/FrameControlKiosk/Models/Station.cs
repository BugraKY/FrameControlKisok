using System.ComponentModel.DataAnnotations;

namespace FrameControlKiosk.Models
{
    public class Station
    {
        [Key]
        public int Id { get; set; }
        public int StationNum { get; set; }//stationLine
        public string Text { get; set; } = string.Empty;//stationName
        public string IP { get; set; } = string.Empty;
        public string Mac { get; set; } = string.Empty;
        public bool IsServer { get; set; }
        public string HexColor { get; set; } = string.Empty;
    }
}
