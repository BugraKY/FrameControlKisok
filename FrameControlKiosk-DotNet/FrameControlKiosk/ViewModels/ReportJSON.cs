using FrameControlKiosk.Models;

namespace FrameControlKiosk.ViewModels
{
    public class ReportJSON
    {
        public List<Station>? Stations { get; set; }
        public List<Control>? Controls { get; set; }
        public List<Definition>? Definitions { get; set; }
        public List<ReportMain>? ReportMains { get; set; }
        public List <ReportDetail>? ReportDetails { get; set; }
        public List <Component>? Components { get; set; }
        public ReportMain? ReportMain { get; set; }
        public List<ReportDetail>? ReportDetail { get; set; }
        public string? reportSelectedDate { get; set; }
        //public DateTime DateOfDownloaded { get; set; }

    }
}
