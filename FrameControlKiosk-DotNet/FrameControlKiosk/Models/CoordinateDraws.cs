using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrameControlKiosk.Models
{
    public class CoordinateDraws
    {
        [Key]
        public long Id { get; set; }
        public string? Coordinates { get; set; } // "x1,y1;x2,y2;x3,y3;..." formatında Örn: (100,200;102,202;105,205;)
        public int ImageType { get; set; }
        public long ReportDetailId { get; set; }
        [NotMapped]
        public string? Number {  get; set; }
    }
}
