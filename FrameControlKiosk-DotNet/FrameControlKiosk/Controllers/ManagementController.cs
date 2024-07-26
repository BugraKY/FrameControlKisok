using FrameControlKiosk.Data;
using FrameControlKiosk.Models;
using FrameControlKiosk.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace FrameControlKiosk.Controllers
{
    public class ManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private string errCompInfo = "</br><h1>Expert Quality Services Kiosk Systems</h1>";
        private string htmlBeforeBody = "<html style='width:100%; height:100%; background-color:#f4eae0' lang=\"tr\"><head></head><body>";
        private string htmlAfterBody = "</body></html>";
        public ManagementController(ApplicationDbContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("managament/readjson/kiosk-reports/{date}")]
        public IActionResult ReadJSON(string date)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                WriteIndented = true // Daha okunaklı bir format için
            };
            try
            {
                WebClient client = new WebClient();
                string value = client.DownloadString("http://127.0.0.1:5000/assets/kiosk-reports/rapor_" + date + ".json");
                var _jsonobj = JsonSerializer.Deserialize<ReportJSON>(value);

                _jsonobj.reportSelectedDate = date;
                return View(_jsonobj);
                //return Json(_jsonobj, options);
            }
            catch (Exception ex)
            {

                return Json("ERR: " + ex.Message + errCompInfo, options);
            }


        }
        [HttpGet("managament/readjson/kiosk-reports/{date}/{reportMainİd}")]
        public IActionResult ReportView(string date, long reportMainİd)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                WriteIndented = true // Daha okunaklı bir format için
            };
            try
            {
                WebClient client = new WebClient();
                string value = client.DownloadString("http://127.0.0.1:5000/assets/kiosk-reports/rapor_" + date + ".json");
                var _jsonobj = JsonSerializer.Deserialize<ReportJSON>(value);

                var reportmain = _jsonobj!.ReportMains!.Find(i => i.Id == reportMainİd);
                //var reportmainList = new List<ReportMain>();
                var reportdetails = _jsonobj.ReportDetails!.Where(i => i.ReportMainId == reportMainİd).ToList();
                var definitions = _jsonobj.Definitions;
                var controls = _jsonobj.Controls;
                var stations = _jsonobj.Stations;

                definitions = definitions!.Join(controls!,
                    d => d.ControlId,
                    c => c.Id,
                    (d, c) => new { def = d, ctrl = c }).Select(x => new Definition
                    {
                        Control = x.ctrl,
                        Id = x.def.Id,
                        ControlId = x.ctrl.Id,
                        StationId = x.def.StationId,
                        DefinitionLine = x.def.DefinitionLine,
                        Shift = x.def.Shift
                    }).AsEnumerable().ToList();

                definitions = definitions.Join(stations!,
                    d => d.StationId,
                    s => s.Id,
                    (d, s) => new { def = d, station = s }).Select(x => new Definition
                    {
                        Control = x.def.Control,
                        Station = x.station,
                        Id = x.def.Id,
                        ControlId = x.def.ControlId,
                        StationId = x.def.StationId,
                        DefinitionLine = x.def.DefinitionLine,
                        Shift = x.def.Shift
                    }).AsEnumerable().ToList();

                reportdetails = reportdetails.Join(definitions,
                r => r.DefinitionId,
                d => d.Id,
                (r, d) => new { report = r, def = d }).Select(x => new ReportDetail
                {
                    Id = x.def.Id,
                    Definition = x.def,
                    DefinitionId = x.def.Id,
                    DefinitionLine = x.report.DefinitionLine,
                    Description = x.report.Description,
                    Ok = x.report.Ok,
                    ReportMainId = x.report.ReportMainId,
                    StationId = x.report.StationId,
                }).AsEnumerable().ToList();


                _jsonobj = new ReportJSON
                {
                    ReportMains = _jsonobj.ReportMains,
                    ReportDetails = _jsonobj.ReportDetails,
                    Controls = _jsonobj.Controls,
                    Definitions = _jsonobj.Definitions,
                    Components = _jsonobj.Components,
                    Stations = _jsonobj.Stations,
                    ReportMain = reportmain,
                    ReportDetail = reportdetails,
                    reportSelectedDate = date
                };

                return View(_jsonobj);
                //return Json(_jsonobj, options);
            }
            catch (Exception ex)
            {

                return Content(htmlBeforeBody + "ERR: " + ex.Message + errCompInfo + htmlAfterBody, " text/html; charset=utf-8");
            }


        }
    }
}
