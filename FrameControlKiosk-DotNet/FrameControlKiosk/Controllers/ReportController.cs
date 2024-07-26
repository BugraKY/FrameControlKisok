using FrameControlKiosk.Data;
using FrameControlKiosk.Models;
using FrameControlKiosk.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace FrameControlKiosk.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string styleTemp = @"
<style>
        body {
            margin: 0;
            font-family: var(--bs-body-font-family);
            font-size: var(--bs-body-font-size);
            font-weight: var(--bs-body-font-weight);
            line-height: var(--bs-body-line-height);
            color: var(--bs-body-color);
            text-align: var(--bs-body-text-align);
            background-color: var(--bs-body-bg);
            -webkit-text-size-adjust: 100%;
            -webkit-tap-highlight-color: transparent;
        }
        table td, table td tr, table td div {
            margin-top: 1px !important;
            margin-bottom: 1px !important;
            padding-top: 1px !important;
            padding-bottom: 1px !important;
            line-height: 1 !important;
        }
        .table th, .table td {
            border: 1px solid black;
            vertical-align: middle;
            padding: 1px;
        }
        table {
            caption-side: bottom;
            border-collapse: collapse;
        }
        tbody, td, tfoot, th, thead, tr {
            border-color: inherit;
            border-style: solid;
            border-width: 0;
        }
        .table>:not(caption)>*>* {
            padding: 0.5rem 0.5rem;
            background-color: var(--bs-table-bg);
            border-bottom-width: 1px;
            box-shadow: inset 0 0 0 9999px var(--bs-table-accent-bg);
        }
        .table {
            --bs-table-bg: transparent;
            --bs-table-accent-bg: transparent;
            --bs-table-striped-color: #212529;
            --bs-table-striped-bg: rgba(0, 0, 0, 0.05);
            --bs-table-active-color: #212529;
            --bs-table-active-bg: rgba(0, 0, 0, 0.1);
            --bs-table-hover-color: #212529;
            --bs-table-hover-bg: rgba(0, 0, 0, 0.075);
            width: 100%;
            margin-bottom: 0 !important;
            color: #212529;
            vertical-align: top;
            border-color: #dee2e6;
        }
        .table-bordered {
            border-collapse: collapse;
        }
        .m-0 {
            margin: 0 !important;
        }
        .row {
            --bs-gutter-x: 1.5rem;
            --bs-gutter-y: 0;
            display: flex;
            flex-wrap: wrap;
            margin-top: calc(var(--bs-gutter-y) * -1);
            margin-right: calc(var(--bs-gutter-x) * -.5);
            margin-left: calc(var(--bs-gutter-x) * -.5);
        }

        .align-items-center {
            align-items: center !important;
        }

        .justify-content-center {
            justify-content: center !important;
        }

        .d-flex {
            display: flex !important;
        }

        .col {
            flex: 1 0 0%;
        }
        .col-1 {
            flex: 0 0 auto;
            width: 8.33333333%;
        }

        .col-2 {
            flex: 0 0 auto;
            width: 16.66666667%;
        }
        .col-3 {
            flex: 0 0 auto;
            width: 25%;
        }
        .col-6 {
            flex: 0 0 auto;
            width: 50%;
        }

        .col-12 {
            flex: 0 0 auto;
            width: 100%;
        }
        .text-center {
            text-align: center;
        }
        svg:not(:host).svg-inline--fa, svg:not(:root).svg-inline--fa {
            overflow: visible;
            box-sizing: content-box;
        }
        .svg-inline--fa {
            display: var(--fa-display,inline-block);
            height: 1em;
            overflow: visible;
            vertical-align: -0.125em;
        }
        img, svg {
            vertical-align: middle;
        }
        .fa-check:before {
            content: ""\f00c"";
        }
        </style>";
        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Route("report/kioskReport/{reportMainId}")]
        public IActionResult KioskReport(long reportMainId)
        {
            var reportMain = _context.ReportMain.FirstOrDefault(x => x.Id == reportMainId);

            if (reportMain == null)
            {
                return View("PageNotFound");
            }
            reportMain!.Component = _context.Component.FirstOrDefault(x => x.FrameID == reportMain.ComponentId);
            var reportDetail = _context.ReportDetail.Where(x => x.ReportMainId == reportMainId).ToList();
            var reportDetailIds = _context.ReportDetail.Where(x => x.ReportMainId == reportMainId).Select(x => x.Id).ToList();
            var definition = _context.Definition.ToList();
            var controls = _context.Control.ToList();
            var stations = _context.Station.ToList();
            var coordDraws = _context.CoordinateDraws.Where(x => reportDetailIds.Contains(x.ReportDetailId)).ToList();
            var coordinates = _context.Coordinate.ToList();
            var coordinateChecks = _context.CoordinateCheck.Where(x => x.ReportMainId == reportMainId).ToList().
                Join(coordinates,
                cc => cc.CoordinateId,
                c => c.Id,
                (cc, c) => new { coordCheck = cc, coordinate = c }).Select(x => new CoordinateCheck
                {
                    Id = x.coordCheck.Id,
                    Coordinate = x.coordinate,
                    CoordinateId = x.coordinate.Id,
                    Marked = x.coordCheck.Marked,
                    ReportDetailId = x.coordCheck.ReportDetailId,
                }).ToList();


            var definitionAnonymous = definition.Join(controls,
                d => d.ControlId,
                c => c.Id,
                (d, c) => new { def = d, ctrl = c });


            definition = definitionAnonymous.Select(x => new Definition
            {
                Id = x.def.Id,
                StationId = x.def.StationId,
                Shift = x.def.Shift,
                ShiftList = x.def.ShiftList,
                Control = x.ctrl,
                ControlId = x.ctrl.Id,
                DefinitionLine = x.def.DefinitionLine,
                Image = x.def.Image,
            }).OrderBy(x => x.DefinitionLine).AsEnumerable().ToList();

            var datadefinition = definition.Join(stations,
                x => x.StationId,
                s => s.Id,
                (x, s) => new { def = x, stn = s }).Select(i => new Definition
                {
                    Id = i.def.Id,
                    Shift = i.def.Shift,
                    ShiftList = i.def.ShiftList,
                    Control = i.def.Control,
                    ControlId = i.def.ControlId,
                    DefinitionLine = i.def.DefinitionLine,
                    Image = i.def.Image,
                    Station = i.stn,
                    StationId = i.stn.Id,
                    Base64 = i.def.Base64,
                    ImageFile = i.def.ImageFile,
                }).OrderBy(x => x.DefinitionLine).AsEnumerable().ToList();


            reportDetail = reportDetail.Join(datadefinition,
                r => r.DefinitionId,
                d => d.Id,
                (r, d) => new { reportdet = r, def = d }).Select(x => new ReportDetail
                {
                    Id = x.reportdet.Id,
                    Definition = x.def,
                    DefinitionId = x.def.Id,
                    DefinitionLine = x.def.DefinitionLine,
                    Description = x.reportdet.Description,
                    Ok = x.reportdet.Ok,
                    StationId = x.reportdet.StationId,
                    ReportMainId = x.reportdet.ReportMainId
                }).ToList();


            reportMain.CoordinateCheck = coordinateChecks.Join(reportDetail,
                cc => cc.ReportDetailId,
                rd => rd.Id,
                (cc, rd) => new { coordChecks = cc, reportDet = rd }).Select(x => new CoordinateCheck
                {
                    Id = x.coordChecks.Id,
                    Saved = x.coordChecks.Saved,
                    Coordinate = x.coordChecks.Coordinate,
                    CoordinateId = x.coordChecks.CoordinateId,
                    Marked = x.coordChecks.Marked,
                    ReportDetail = x.reportDet,
                    ReportDetailId = x.coordChecks.ReportDetailId,
                    ReportMainId = x.coordChecks.ReportMainId
                }).ToList();

            if (coordDraws.Count > 0)
            {
                foreach (var coordDraw in coordDraws)
                {
                    reportMain.CoordinateCheck.Add(
                        new CoordinateCheck
                        {
                            Id = 1,
                            Coordinate = new Coordinate
                            {
                                Id = 1,
                                ImageType = coordDraw.ImageType,
                                MarkSize = 0,
                                X = 0,
                                Y = 0,
                            },
                            ReportDetailId = coordDraw.ReportDetailId,
                            ReportDetail = reportDetail.FirstOrDefault(x => x.Id == coordDraw.ReportDetailId)
                        });
                }

            }

            reportMain.ReportDetails = reportDetail;


            return View(reportMain);
            //return null;
        }

        public IActionResult ReportPageNotFound()
        {
            return View();
        }

        [HttpPost("report/kioskReportpdf")]
        public async Task<IActionResult> ReportConvertPdfAsync(string HtmlString)
        {
            try
            {
                var HtmlTemplate = styleTemp + HtmlString;
                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                }))
                {
                    using (var page = await browser.NewPageAsync())
                    {

                        await page.SetContentAsync(HtmlTemplate);
                        var pdfStream = await page.PdfStreamAsync(
                            new PdfOptions
                            {
                                Format = PaperFormat.A4,
                                MarginOptions = new PuppeteerSharp.Media.MarginOptions()
                                { Bottom = "15px", Left = "15px", Right = "15px", Top = "15px" }
                            });


                        return File(pdfStream, "application/pdf");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message, "text/html");
            }
            return NoContent();
        }

        [Route("report/Index/select-date")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("report/get-reports/{selectDate}/{search?}")]
        public List<ReportMain> GetReportList(string selectDate, string? search = null)
        {
            var _selectDate = DateTime.Parse(selectDate);
            //var daily = DateTime.Now.AddDays(-1).AddTicks(-1);
            var lastofHour = DateTime.Parse(_selectDate.ToString("dd/MM/yyyy 23:59:59")).ToUniversalTime();
            var firstofHour = DateTime.Parse(_selectDate.ToString("dd/MM/yyyy 00:00:00")).ToUniversalTime();
            var reportDaily = new List<ReportMain>();
            if (!string.IsNullOrEmpty(search))
            {
                reportDaily = _context.ReportMain.Where(p => p.FrameNo!.Contains(search)).ToList();
            }
            else { reportDaily = _context.ReportMain.Where(p => p.StartingDate <= lastofHour && p.StartingDate >= firstofHour).OrderByDescending(x => x.Id).ToList(); }

            return reportDaily;
        }
    }
}

