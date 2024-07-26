using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrameControlKiosk.Models
{
    public class ReportDetail
    {
        [Key]
        public long Id { get; set; }
        public long DefinitionId { get; set; }
        [NotMapped]
        public Definition? Definition { get; set; }
        //public int StationId { get; set; }
        public long DefinitionLine { get; set; }
        public int StationId { get; set; }
        public long ReportMainId { get; set; }
        public string? Description { get; set; }
        public bool Ok { get; set; }//Rapor detayı uygunsa OK True olacak.
    }
}
