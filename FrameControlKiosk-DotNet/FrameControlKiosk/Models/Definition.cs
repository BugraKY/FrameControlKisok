using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrameControlKiosk.Models
{
    public class Definition
    {
        [Key]
        public long Id { get; set; }
        public int ControlId { get; set; }       //Veriler ilişkili değil. Bunu entityde yapacağız.
        public int StationId { get; set; }       //Veriler ilişkili değil. Bunu entityde yapacağız.
        public int ComponentId { get; set; }
        public int DefinitionLine { get; set; }  //İstasyon içerisindeki kontolün sırası 
        public string? Shift { get; set; }
        public bool Image { get; set; }
        [NotMapped]
        public string? Base64 { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; }
        [NotMapped]
        public string[]? ShiftList { get; set; }
        [NotMapped]
        public Control? Control { get; set; }
        [NotMapped]
        public Station? Station { get; set; }
    }
}
