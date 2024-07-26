using FrameControlKiosk.Models;

namespace FrameControlKiosk.ViewModels
{
    public class StationVM
    {
        public Station? Station { get; set; }
        public int StationId { get; set; }
        public IEnumerable<Definition>? Definitions { get; set; }
        public ReportDetail? ReportDetail { get; set; }
        public long ReportDetailDefinitionId { get; set; }
        public long ReportMainId { get; set; }
        public int ComponentId { get; set; }
        public string? FrameNo { get; set; }
        public int ProcessingForm { get; set; } = 0;
        //public bool CurrentReport { get; set; }
        public bool NextStation { get; set; }
        public bool AwaitingNextStation { get; set; }
        public bool AwaitingBeforeStation { get; set; }
        public bool TakenOver { get; set; }
        //public object? coordinateList { get; set; }
        public string? Description { get; set; }
        public bool Ok { get; set; }
        public List<CoordinateCheck> coordinatesList { get; set; }
        public List<DrawList> DrawList { get; set; }
        public List<Component> components { get; set; }
        public Component Component { get; set; }
    }
    public class DrawList
    {
        public string CoordString { get; set; }
        public string CanvasId { get; set; }
    }
}
