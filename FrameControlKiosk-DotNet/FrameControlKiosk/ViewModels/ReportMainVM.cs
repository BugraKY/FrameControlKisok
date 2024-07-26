using FrameControlKiosk.Models;

namespace FrameControlKiosk.ViewModels
{
    public class ReportMainVM : ReportMain
    {
        public string? ReportDate { get; set; }
        public string? StartingReportTime { get; set; }
        public string? EndingReportTime { get; set; }
        public int Nok { get; set; }
        public int DoneCount { get; set; }
    }
}
