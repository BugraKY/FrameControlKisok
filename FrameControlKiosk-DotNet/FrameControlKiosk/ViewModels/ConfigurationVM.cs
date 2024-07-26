using FrameControlKiosk.Models;

namespace FrameControlKiosk.ViewModels
{
    public class ConfigurationVM
    {
        public List<Station>? Stations { get; set; }
        public List<Control>? Controls { get; set; }
        public List<Definition>? Definition { get; set; }
        public Station? Station { get; set; }
        public List<Control>? DefinitedControls { get; set; }
        public List<Component>? Components { get; set; }
        public Component? Component { get; set; }
        public int ControlId { get; set; }      //Veriler ilişkili değil. Bunu entityde yapacağız.
        public int StationId { get; set; }      //Veriler ilişkili değil. Bunu entityde yapacağız.
    }
}
