using System.ComponentModel.DataAnnotations;

namespace FrameControlKiosk.Models
{
    public class Control
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}