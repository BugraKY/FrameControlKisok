//using DocumentFormat.OpenXml.Bibliography;
//using DocumentFormat.OpenXml.InkML;
//using DocumentFormat.OpenXml.Spreadsheet;
using FrameControlKiosk.Data;
using FrameControlKiosk.Extensions;
using FrameControlKiosk.Models;
using FrameControlKiosk.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using static System.Collections.Specialized.BitVector32;

namespace FrameControlKiosk.Controllers
{
    public class StationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StationsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [Route("station")]
        public IActionResult Index()
        {
            return View();
        }
        [Route("station/{num}")]
        public IActionResult StationNumber(int num)
        {
            var station = _context.Station.Where(i => i.StationNum == num).FirstOrDefault();//burada join kullanılarak hem definition hemde controls çekilmeli!!
            var components = _context.Component.Where(x => x.Active).ToList();
            var Stations = _context.Station.OrderBy(i => i.StationNum).ToList();
            var LastStatin = Stations.Last();
            var currentIndex = Stations.IndexOf(station!);


            var definitions = _context.Definition.Where(i => i.StationId == station!.Id);
            var controls = _context.Control.Where(i => i.Active);
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            //bool verify = AppConstant.DetectedDevices!.Any(i => Services.NormalizeMacAddress(i.MacAddress!) == Services.NormalizeMacAddress(station!.Mac) && i.Ip == station.IP);
            bool verify = false;
            if (!verify)
            {
                //Services.DetecthDevices(_context);

                if (ipAddress!.ToString() == station!.IP || station.IsServer)
                    verify = true;
            }


            var definitionAnonymous = definitions.Join(controls,
                d => d.ControlId,
                c => c.Id,
                (d, c) => new { def = d, ctrl = c });

            var coordinates = _context.Coordinate.ToList();
            var coordinateChecks = new List<CoordinateCheck>();


            var componentEmpty = new Models.Component { };
            var stationVM = new StationVM
            {
                Station = station,
                Definitions = definitionAnonymous.Select(x => new Definition
                {
                    Id = x.def.Id,
                    Shift = x.def.Shift,
                    ShiftList = x.def.ShiftList,
                    Control = x.ctrl,
                    ControlId = x.ctrl.Id,
                    DefinitionLine = x.def.DefinitionLine,
                    Image = x.def.Image,
                    ComponentId = x.def.ComponentId
                }).OrderBy(x => x.DefinitionLine).AsEnumerable(),
                components = components,
                Component = componentEmpty,
            };
            definitions = stationVM.Definitions.AsQueryable();


            var NumOfStation = _context.Station.Count();//istasyon adedi
            var NumOfDefinitions = _context.Definition.Where(i => i.StationId == station!.Id).Count();//istasyona tanımlı kontroller

            //var ReportMain = _context.ReportMain.Last(i => i.StationId == station!.Id && i.Done == false && i.ProcessingDone == false && i.Cancelled == false);
            var ReportMainCurrentStation = _context.ReportMain
                .Where(i => i.StationId == station!.Id && i.Done == false && i.ProcessingDone == false && i.Cancelled == false)
                .OrderByDescending(i => i.StartingDate)
                .LastOrDefault();

            var ReportMainNextStation = _context.ReportMain
                .Where(i => i.StationId == station!.Id && i.Done == false && i.ProcessingDone == true && i.Cancelled == false)
                .OrderByDescending(i => i.StartingDate)
                .LastOrDefault();

            //null ise yeni rapor hazırlanacak değil ise var olan rapora devam edilecek.


            if (ReportMainCurrentStation == null && ReportMainNextStation == null && currentIndex > 0)
                stationVM.AwaitingBeforeStation = true;
            if (ReportMainCurrentStation != null)
            {
                NumOfDefinitions = _context.Definition.Where(i => i.StationId == station!.Id && i.ComponentId == ReportMainCurrentStation.ComponentId).Count();
                var reportDetails = _context.ReportDetail.Where(i => i.ReportMainId == ReportMainCurrentStation.Id);//reportmaib null değilse mutlaka reoırtdetail dolu olmalı!
                //stationVM.ProcessingForm = reportDetails.Count();
                //stationVM.ProcessingForm = definitions.Count();
                stationVM.ProcessingForm = (int)reportDetails.OrderBy(i => i.Id).LastOrDefault(i => i.ReportMainId == ReportMainCurrentStation.Id && i.StationId == station!.Id)!.DefinitionLine;
                stationVM.ReportMainId = ReportMainCurrentStation.Id;
                if (ReportMainCurrentStation.FrameNo != null)
                {
                    stationVM.FrameNo = ReportMainCurrentStation.FrameNo;
                    stationVM.ComponentId = (int)ReportMainCurrentStation.ComponentId;
                    stationVM.Component = _context.Component.FirstOrDefault(x => x.FrameID == (int)ReportMainCurrentStation.ComponentId)!;
                    //definitions = _context.Definition.Where(i => i.StationId == station!.Id && i.ComponentId == stationVM.ComponentId);


                    stationVM.Definitions = definitions.Where(i => i.ComponentId == stationVM.ComponentId);
                    /*

                    #region Coordinate Check
                    //coordinateChecks = _context.CoordinateCheck.Where(i => i.ReportMainId == ReportMainCurrentStation.Id).ToList();
                    coordinateChecks = new List<CoordinateCheck>();
                    var coordIds = _context.CoordinateCheck.Where(i => i.ReportMainId == ReportMainCurrentStation.Id).Select(X => X.CoordinateId).ToList();
                    Console.WriteLine("TEST BASLIYOR...");
                    foreach (var coordId in coordinates.Select(x => x.Id))
                    {
                        if (!coordIds.Contains(coordId))
                        {
                            var coordCheckItem = new CoordinateCheck
                            {
                                CoordinateId = coordId,
                                Marked = false,
                                Saved = false,
                                ReportMainId = ReportMainCurrentStation.Id,
                            };
                            coordinateChecks.Add(coordCheckItem);
                            Console.WriteLine("Eklenecek:" + coordId);
                        }
                    }
                    if (coordinateChecks.Count() > 0)
                    {
                        _context.CoordinateCheck.AddRange(coordinateChecks);
                        _context.SaveChanges();
                    }

                    Console.WriteLine("TEST BITTI. SaveChanges burada olacak. Yeni eklenecek koordinat bilgisi [" + coordinateChecks.Count() + "] Adet");

                    #endregion Coordinate Check

                    */
                }
                else
                    stationVM.FrameNo = "Tanımlanmamış";
            }
            else if (ReportMainNextStation != null)
            {
                var reportDetails = _context.ReportDetail.Where(i => i.ReportMainId == ReportMainNextStation.Id);//reportmaib null değilse mutlaka reoırtdetail dolu olmalı!
                stationVM.ProcessingForm = ReportMainNextStation!.ProcessedLine;
                stationVM.ReportMainId = ReportMainNextStation.Id;


                stationVM.ComponentId = (int)ReportMainNextStation!.ComponentId;
                stationVM.Component = _context.Component.FirstOrDefault(x => x.FrameID == (int)ReportMainNextStation!.ComponentId)!;
                /*definitions = _context.Definition.Where(i => i.StationId == station!.Id && i.ComponentId == stationVM.ComponentId);
                stationVM.Definitions = definitions;*/

                stationVM.Definitions = definitions.Where(i => i.ComponentId == stationVM.ComponentId);


                stationVM.NextStation = true;
                if (ReportMainNextStation.FrameNo != null)
                    stationVM.FrameNo = ReportMainNextStation.FrameNo;
                else
                    stationVM.FrameNo = "Tanımlanmamış";
            }



            if (num == 0)
                return Redirect("/station");
            /*
            else if (!verify)
                return Content(AppConstant.htmlContent, "text/html; charset=utf-8");*/
            if (station != null) //if (station != null && verify)
                return View(stationVM);
            return Redirect("/station/notfound");
        }
        [Route("station/notfound")]
        public IActionResult StationNotFound()
        {
            var remoteRealIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            var htmlContent = $"<!DOCTYPE html><html><head><meta charset='UTF-8'></head><body><h1>Station Not Found</h1><p>The station is unidentified or incorrectly configured.</p><p>İstasyon tanımlanmamış veya doğru bir şekilde konfigüre edilmemiş. Lütfen sunucu tarafındaki konfigurasyon bilgilerini kontrol ediniz.</p><p>Bu cihazın Ip Adresi: {remoteRealIp}</p>{AppConstant.redirectScript}</body></html>";
            return Content(htmlContent, "text/html");
        }
        [HttpPost("station/sendReport")]
        public IActionResult UpsertReport([FromBody] StationVM stationVM)
        {
            //return NoContent();
            var Station = _context.Station.FirstOrDefault(i => i.Id == stationVM.StationId);
            var Definition = _context.Definition.FirstOrDefault(i => i.Id == stationVM.ReportDetailDefinitionId);
            var definitions = _context.Definition.Where(i => i.StationId == Station!.Id && i.ComponentId == stationVM.ComponentId);
            var controls = _context.Control.Where(i => i.Active);
            var Done = false;
            var coordinateChanged = false;
            var processingDone = false;
            var nextStationAwaiting = false;
            var NumOfStation = _context.Station.Count();
            var NumOfDefinitions = _context.Definition.Where(i => i.StationId == Station!.Id).Count();
            var reportDetails = _context.ReportDetail.Where(i => i.ReportMainId == stationVM.ReportMainId);
            var definitionAnonymous = definitions.Join(controls,
                d => d.ControlId,
                c => c.Id,
                (d, c) => new { def = d, ctrl = c });

            //return NoContent();

            if (stationVM.ReportMainId == 0 && Station!.StationNum == 1)
            {
                var reportMain = new ReportMain
                {
                    StartingDate = DateTime.UtcNow,
                    FrameNo = stationVM.FrameNo,
                    StationId = Station.Id,
                    ComponentId = stationVM.ComponentId,//burası şimdilik sabit ancak, sonraki güncelleştirmede gerçek veri yapısından buraya kaydedilmeli.
                };
                //return NoContent();
                _context.Add(reportMain);
                _context.SaveChanges();
                stationVM.ReportMainId = reportMain.Id; //Rapor hazırlamaya başlandığında döndürülecek reportmainid!!
            }
            else
            {
                //return NoContent();
                var reportMain = _context.ReportMain.FirstOrDefault(i => i.Id == stationVM.ReportMainId);
                NumOfDefinitions = _context.Definition.Where(i => i.StationId == stationVM.StationId && i.ComponentId == reportMain!.ComponentId).Count();
                Definition = _context.Definition.FirstOrDefault(i => i.Id == stationVM.ReportDetailDefinitionId && i.ComponentId == reportMain!.ComponentId);
                reportMain!.ProcessingDone = false;

                stationVM.ComponentId = (int)reportMain.ComponentId;
                stationVM.ProcessingForm = reportMain.ProcessedLine;





                /*
                if (reportMain!.ProcessedLine == NumOfDefinitions - 1)
                {
                    //reportMain.ProcessedLine = Station!.StationNum;
                    reportMain.EndingDate = DateTime.UtcNow;
                    //reportMain.Done = true;
                    reportMain.Cancelled = false;
                }*/




                var Stations = _context.Station.OrderBy(i => i.StationNum).ToList();
                var currentIndex = Stations.IndexOf(Station!);
                var nextStation = new Station();
                var def_say = definitions.Count() - 2;

                if(Station.StationNum>1)
                    def_say = definitions.Count() - 1;

                if (currentIndex + 1 < Stations.Count())
                {
                    nextStation = Stations[currentIndex + 1];

                    if (stationVM.ProcessingForm == def_say)
                    {
                        nextStationAwaiting = _context.ReportMain.Any(i => i.StationId == nextStation.Id && i.Cancelled == false && i.Done == false);
                    };

                }

                var lastReportDetail = reportDetails.OrderBy(i => i.Id).LastOrDefault(i => i.ReportMainId == reportMain.Id && i.StationId == Station!.Id);
                stationVM.ProcessingForm = (int)(lastReportDetail?.DefinitionLine ?? 0);
                if (stationVM.ProcessingForm == (NumOfDefinitions - 1) && !nextStationAwaiting)//if (reportDetails.Count() == (NumOfDefinitions - 1))
                {
                    processingDone = true;
                    reportMain.EndingDate = DateTime.UtcNow;
                    reportMain.Cancelled = false;
                    reportMain.ProcessingDone = true;
                    reportMain.StationId = nextStation.Id;
                    reportMain.ProcessedLine = 0;
                    if (reportMain.StationId == 0)
                        reportMain.Done = true;
                }
                else
                    reportMain.ProcessedLine++;

                if (reportMain != null)
                {
                    _context.ReportMain.Update(reportMain);
                }
            }
            var reportDetail = new ReportDetail
            {
                DefinitionId = stationVM.ReportDetailDefinitionId,
                Description = stationVM.Description,
                Ok = stationVM.Ok,
                ReportMainId = stationVM!.ReportMainId,
                DefinitionLine = Definition!.DefinitionLine,
                StationId = Station!.Id,

            };
            _context.ReportDetail.Add(reportDetail);
            if (!nextStationAwaiting && stationVM.ReportMainId != 0)//stationVM.ReportMainId != 0 sonradan eklendi
            {
                _context.SaveChanges(); 
            }
            var reportDetailId = reportDetail.Id;
            var ReportMainId = reportDetail.ReportMainId;

            #region SaveCoordinate
            if (!string.IsNullOrWhiteSpace(stationVM.Description) && stationVM.coordinatesList != null)
            {
                var coordinates = new List<CoordinateCheck>();
                foreach (var item in stationVM.coordinatesList)
                {

                    if (item.Marked && !item.Saved)
                    {
                        coordinateChanged = true;
                        item.Saved = true;
                        item.ReportDetailId = reportDetailId;
                        item.ReportMainId = ReportMainId;
                        coordinates.Add(item);
                    }

                }
                if (coordinateChanged && reportDetailId != 0)
                {
                    _context.CoordinateCheck.UpdateRange(coordinates);
                    _context.SaveChanges();
                }

                stationVM.coordinatesList = coordinates;
            }
            if (stationVM.DrawList != null)
            {
                if (stationVM.DrawList.Count() > 0)
                {
                    var _coordinateDraws = new List<CoordinateDraws>();
                    foreach (var draw in stationVM.DrawList)
                    {
                        _coordinateDraws.Add(new CoordinateDraws
                        {
                            Coordinates = draw.CoordString,
                            ImageType = draw.CanvasId == "frontCanvas" ? 1 : (draw.CanvasId == "rearCanvas" ? 2 : 0),
                            ReportDetailId = reportDetailId
                        });
                    }
                    if (reportDetailId != 0)
                    {
                        _context.CoordinateDraws.AddRange(_coordinateDraws);
                        _context.SaveChanges();
                    }

                }
            }
            #endregion SaveCoordinate
            var reportDetailIds = _context.ReportDetail.Where(x => x.ReportMainId == ReportMainId).Select(x => x.Id).ToList();
            var coordinateDraws = _context.CoordinateDraws.Where(x => reportDetailIds.Contains(x.ReportDetailId)).ToList();
            stationVM.Definitions = definitionAnonymous.Select(x => new Definition
            {
                Id = x.def.Id,
                Shift = x.def.Shift,
                ShiftList = x.def.ShiftList,
                Control = x.ctrl,
                ControlId = x.ctrl.Id,
                DefinitionLine = x.def.DefinitionLine,
            }).OrderBy(x => x.DefinitionLine).AsEnumerable();

            //return NoContent();
            //return View(stationVM);//returnviewa mutlaka bir ReportMainId yönlendirebilmemiz gerekiyor. Tabikide reportmain oluşturulduysa
            //return View("StationNumber", stationVM);
            return Json(new
            {
                reportMainId = stationVM.ReportMainId,
                done = Done,
                processingDone = processingDone,
                nextStationAwaiting = nextStationAwaiting,
                coordinates = stationVM.coordinatesList,
                coordinateDraws = coordinateDraws

            });
        }
        [Route("station/DownloadJson")]
        public IActionResult DownloadJson()
        {
            var control = _context.Control.ToList();
            var definition = _context.Definition.ToList();
            var reportDetail = _context.ReportDetail.ToList();
            var reportMain = _context.ReportMain.ToList();
            var station = _context.Station.ToList();
            var allData = new List<object> { control, definition, reportDetail, reportMain, station };

            // JSON'a serileştirme seçenekleri
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Bu satırı ekleyin
            };


            string jsonString = JsonSerializer.Serialize(allData, options);

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
            string fileName = "kisok_data.json";
            return File(fileBytes, "application/json", fileName);
        }
        public List<ReportMain> ListReports(DateTime startingDateTime)
        {
            return _context.ReportMain.Where(i => i.StartingDate == startingDateTime).ToList();
        }
        public IActionResult DownloadExcell(string Id)
        {

            return View();
        }
        [HttpGet("station/getreport/{date}")]
        public List<ReportMainVM> ShowReport(string date)
        {
            var reports = new List<ReportMain>();
            var reportsVm = new List<ReportMainVM>();
            int index = date.IndexOf(" GMT");
            if (index > 0)
            {
                date = date.Substring(0, index).Trim();
            }
            DateTime startingDateTime;
            DateTime endingDateTime;
            try
            {
                //dateTime = DateTime.Parse(date);
                //devam edeceğiz!!!
                startingDateTime = DateTime.SpecifyKind(DateTime.Parse(date), DateTimeKind.Utc);
                endingDateTime = DateTime.SpecifyKind(startingDateTime.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                reports = _context.ReportMain.Where(i => i.StartingDate >= startingDateTime && i.StartingDate <= endingDateTime).ToList();
                reportsVm = reports.Select(i => new ReportMainVM
                {
                    //StartingDate = i.StartingDate
                    Id = i.Id,
                    StartingDate = i.StartingDate,
                    EndingDate = i.EndingDate,
                    ReportDate = i.StartingDate.ToString("dd/MM/yyyy"),
                    StartingReportTime = i.StartingDate.ToString("HH:mm"),
                    EndingReportTime = i.EndingDate.ToString("HH:mm"),
                    Done = i.Done,
                    Nok = _context.ReportDetail.Where(x => x.ReportMainId == i.Id && !x.Ok).Count(),
                    DoneCount = _context.ReportDetail.Where(x => x.ReportMainId == i.Id).Count(),
                    //diğer değişkenler..
                }).ToList();
            }
            catch (FormatException)
            {
                
                Console.WriteLine($"Failed to parse date: {date}");
                reportsVm = new List<ReportMainVM>();
            }

            return reportsVm;
        }

        [HttpPost("station/getreport-details")]
        public object ShowReportDetail([FromBody] ReportMain reportMain)
        {
            var control = _context.Control.ToList();
            var definitionAnonymous = _context.Definition.Join(control,
                d => d.ControlId,
                c => c.Id,
                (d, c) => new { def = d, ctrl = c });

            var definitions = definitionAnonymous.Select(x => new Definition
            {
                Id = x.def.Id,
                Shift = x.def.Shift,
                ShiftList = x.def.ShiftList,
                Control = x.ctrl,
                ControlId = x.ctrl.Id,
                DefinitionLine = x.def.DefinitionLine,
                Image = x.def.Image
            }).OrderBy(x => x.DefinitionLine).AsEnumerable().ToList();

            reportMain = _context.ReportMain.FirstOrDefault(i => i.Id == reportMain.Id);
            var reportDetail = _context.ReportDetail.Where(i => i.ReportMainId == reportMain!.Id);
            return reportDetail;
        }
        [HttpPost("station/getCoords/{reportMainId}")]
        public object getCoordinates(long reportMainId)
        {
            var coordinates = _context.Coordinate.ToList();
            var coordinateChecks = new List<CoordinateCheck>();


            var reportDetails = _context.ReportDetail.Where(x => x.ReportMainId == reportMainId).ToList();
            var definition = _context.Definition.ToList();
            var controls = _context.Control.ToList();
            var stations = _context.Station.ToList();

            /*
            var reportDetails = _context.ReportDetail.Where(x => x.ReportMainId == reportMainId).
                Join(defs,
                rd => rd.DefinitionId,
                d => d.Id,
                (rd, d) => new { repdet = rd, def = d }).Select(x=>new ReportDetail
                {
                    Id=x.repdet.Id,
                    Definition=x.def,
                    DefinitionId=x.def.Id,
                    DefinitionLine=x.repdet.DefinitionLine,
                    Description=x.repdet.Description,
                    Ok=x.repdet.Ok,
                    ReportMainId=reportMainId,
                    StationId=x.repdet.StationId
                }).AsEnumerable().ToList();*/




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


            reportDetails = reportDetails.Join(datadefinition,
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
                }).AsEnumerable().ToList();










            string filePath = Path.Combine(_env.WebRootPath, "assets", "coordinates-schema.json");

            string json = System.IO.File.ReadAllText(filePath);


            var CoordinateChecksJSON = JsonSerializer.Deserialize<List<CoordinateCheck>>(json);
            var coordinatesJSON = CoordinateChecksJSON!.Select(x => x.Coordinate).ToList();

            //_context.AddRange(coordinatesJSON);
            //_context.SaveChanges();


            foreach (var coordinateCheck in CoordinateChecksJSON)
            {

                var matchingCoordinate = coordinates.FirstOrDefault(c => c.X == coordinateCheck.Coordinate.X && c.Y == coordinateCheck.Coordinate.Y);

                if (matchingCoordinate != null)
                {
                    coordinateCheck.Coordinate.Id = matchingCoordinate.Id;
                    coordinateCheck.CoordinateId = matchingCoordinate.Id;
                }
            }
            var markedCoordinates = _context.CoordinateCheck.Where(i => i.ReportMainId == reportMainId).ToList();

            foreach (var coordinateCheckJSON in CoordinateChecksJSON)
            {
                var matchingMarkedCoordinate = markedCoordinates
                    .FirstOrDefault(mc => mc.CoordinateId == coordinateCheckJSON.CoordinateId
                                          && mc.Marked
                                          && mc.Saved);

                if (matchingMarkedCoordinate != null)
                {
                    coordinateCheckJSON.Marked = true;
                    coordinateCheckJSON.Saved = true;
                    coordinateCheckJSON.ReportDetailId = matchingMarkedCoordinate.ReportDetailId;
                    coordinateCheckJSON.ReportDetail = reportDetails.First(x => x.Id == matchingMarkedCoordinate.ReportDetailId);
                }
            }

            // var testcoords = CoordinateChecksJSON.Where(x => x.Saved && x.Marked).ToList();



            return CoordinateChecksJSON;
            //return null;
            /*
            #region Coordinate Check
            //coordinateChecks = _context.CoordinateCheck.Where(i => i.ReportMainId == ReportMainCurrentStation.Id).ToList();
            coordinateChecks = new List<CoordinateCheck>();
            var coordIds = _context.CoordinateCheck.Where(i => i.ReportMainId == reportMainId).Select(X => X.CoordinateId).ToList();
            Console.WriteLine("TEST BASLIYOR...");
            foreach (var coordId in coordinates.Select(x => x.Id))
            {
                if (!coordIds.Contains(coordId))
                {
                    var coordCheckItem = new CoordinateCheck
                    {
                        CoordinateId = coordId,
                        Marked = false,
                        Saved = false,
                        ReportMainId = reportMainId,
                    };
                    //coordinateChecks.Add(coordCheckItem);
                    //Console.WriteLine("Eklenecek:" + coordId);
                }
            }
            if (coordinateChecks.Count() > 0)
            {
                _context.CoordinateCheck.AddRange(coordinateChecks);
                _context.SaveChanges();
            }
            var coordinateCheckss = _context.CoordinateCheck.Where(i => i.ReportMainId == reportMainId).ToList();//Burası düzenlenecek..
            var data = coordinateCheckss.Join(coordinates,
            ch => ch.CoordinateId,
            cor => cor.Id,
            (ch, cor) => new { coordinateChk = ch, coordinate = cor }).
            Select(x => new CoordinateCheck
            {
                Coordinate = x.coordinate,
                CoordinateId = x.coordinateChk.CoordinateId,
                Id = x.coordinateChk.Id,
                Marked = x.coordinateChk.Marked,
                Saved = x.coordinateChk.Saved,
                ReportDetailId = x.coordinateChk.ReportDetailId,
                ReportMainId = x.coordinateChk.ReportMainId
            });





            Console.WriteLine("TEST BITTI. SaveChanges burada olacak. Yeni eklenecek koordinat bilgisi [" + coordinateChecks.Count() + "] Adet");

            #endregion Coordinate Check
            */


        }
        [HttpPost("station/getDraws/{reportMainId}")]
        public object getDraws(long reportMainId)
        {
            var reportDetails = _context.ReportDetail.Where(x => x.ReportMainId == reportMainId).ToList();
            var controls = _context.Control.ToList();
            var stations = _context.Station.ToList();
            var definition = _context.Definition.ToList();
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


            reportDetails = reportDetails.Join(datadefinition,
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
                }).AsEnumerable().ToList();


            var reportDetailIds = _context.ReportDetail.Where(x => x.ReportMainId == reportMainId).Select(x => x.Id).ToList();

            var draws = _context.CoordinateDraws.Where(x => reportDetailIds.Contains(x.ReportDetailId)).ToList();

            foreach (var draw in draws)
            {
                var reportDetail = reportDetails.FirstOrDefault(x => x.Id == draw.ReportDetailId);
                draw.Number = reportDetail?.Definition?.Control?.Id.ToString() ?? "";
            }

            return draws;

        }
        [HttpPost("station/cancelReport/{reportMainId}")]
        public object cancelReport(long reportMainId)
        {
            //return true;
            var reportMain = _context.ReportMain.First(i => i.Id == reportMainId);
            reportMain.Cancelled = true;
            _context.ReportMain.Update(reportMain);
            _context.SaveChanges();
            reportMain = _context.ReportMain.First(i => i.Id == reportMainId);
            return reportMain.Cancelled;
        }
        [Route("station/print/{reportMainId}")]
        public IActionResult Print(long reportMainId)
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

        public IActionResult PageNotFound()
        {
            return View();
        }

        [Route("station/download-report-json/{date}")]
        public IActionResult DownloadReport_json(string date)
        {
            //var date = objectVM.Message;
            int index = date!.IndexOf(" GMT");
            if (index > 0)
            {
                date = date.Substring(0, index).Trim();
            }
            DateTime startingDateTime;
            DateTime endingDateTime;

            startingDateTime = DateTime.SpecifyKind(DateTime.Parse(date), DateTimeKind.Utc);
            endingDateTime = DateTime.SpecifyKind(startingDateTime.AddDays(1).AddTicks(-1), DateTimeKind.Utc);


            var controls = _context.Control.ToList();
            var stations = _context.Station.ToList();
            var definitions = _context.Definition.ToList();

            var reportMain = _context.ReportMain.Where(i => i.StartingDate >= startingDateTime && i.StartingDate <= endingDateTime).ToList();

            var reportMainIds = reportMain.Select(rm => rm.Id).ToList();
            var reportDetail = _context.ReportDetail.Where(rd => reportMainIds.Contains(rd.ReportMainId)).ToList();
            var components = _context.Component.ToList();

            var reportJSON = new ReportJSON
            {
                Controls = controls,
                Stations = stations,
                Definitions = definitions,
                ReportMains = reportMain,
                ReportDetails = reportDetail.OrderBy(x => x.Id).ToList(),
                Components = components
            };

            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                WriteIndented = true // Daha okunaklı bir format için
            };

            string jsonString = JsonSerializer.Serialize(reportJSON, options);

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

            string fileName = "rapor_" + startingDateTime.ToString("ddMMyy") + ".json";

            Response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");
            return File(fileBytes, "application/json", fileName);
        }

        [HttpPost("api/command/open-file-explorer")]
        public IActionResult OpenFileExplorer()
        {
            try
            {
                LinuxShell.OpenNautilus("/home/station1/Downloads");//buraya string dosyayolu appsettings.jsondan alınabilir.
                return Ok("File explorer opened");
            }
            catch (Exception ex)
            {
                return BadRequest("Error opening file explorer: " + ex.Message);
            }
        }
        [HttpGet("api/command/get-usbInfo")]
        public List<string>? GetUsbInfo()
        {
            try
            {
                var usbInfo = LinuxShell.GetConnectedUSBDevicesAsync();//buraya string dosyayolu appsettings.jsondan alınabilir.
                if (usbInfo.Count > 0)
                    return usbInfo!;
            }
            catch (Exception ex)
            {
                return new List<string> { ex.Message };
            }
            return null;
        }

    }
}
