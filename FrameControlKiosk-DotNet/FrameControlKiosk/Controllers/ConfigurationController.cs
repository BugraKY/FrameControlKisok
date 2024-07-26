using FrameControlKiosk.Data;
using FrameControlKiosk.Models;
using FrameControlKiosk.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;

namespace FrameControlKiosk.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        public ConfigurationController(ApplicationDbContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [Route("configuration/main")]
        public IActionResult Index()
        {
            var Stations = _context.Station.OrderBy(i => i.StationNum).ToList();
            var ConfigVM = new ConfigurationVM
            {
                Controls = _context.Control.ToList(),
                Stations = Stations,
                Definition = new List<Definition>(),
                Components = _context.Component.Where(x => x.Active).ToList(),
            };
            var test = _context.Definition.AsEnumerable().Join(Stations,
                d => d.StationId,
                s => s.Id,
                (d, s) => new { definition = d, station = s }).Select(i => i.definition).ToList();
            return View(ConfigVM);
        }
        [Route("configuration/definition/{stationId}/{frameId}")]
        public IActionResult EditDefinition(int stationId, int frameId)
        {
            try
            {
                var definition = _context.Definition.Where(i => i.StationId == stationId && i.ComponentId == frameId).ToList();
                var station = _context.Station.FirstOrDefault(i => i.Id == stationId);
                var controls = _context.Control.ToList();

                var definitionsWithControl = definition.Select(d => new Definition
                {
                    Id = d.Id,
                    ControlId = d.ControlId,
                    StationId = d.StationId,
                    DefinitionLine = d.DefinitionLine,
                    Shift = d.Shift,
                    Control = controls.FirstOrDefault(c => c.Id == d.ControlId),
                    Station = station,
                    Image = d.Image
                }).OrderBy(i => i.DefinitionLine).ToList();


                var ConfigVM = new ConfigurationVM
                {
                    Definition = definitionsWithControl,
                    StationId = stationId,
                    Controls = _context.Control.ToList(),
                    Station = station,
                    Component = _context.Component.FirstOrDefault(x => x.FrameID == frameId)

                };
                return View(ConfigVM);
            }
            catch (Exception ex)
            {
                var innerexp = "";
                var source = "";
                if (ex.InnerException != null && ex.InnerException.StackTrace != null)
                {
                    innerexp = ex.InnerException.Message;
                    source = ex.InnerException.StackTrace;
                    return Content("HATA:" + ex.Message + "<br/>" + "<h3>Lütfen buradaki hata mesajlarını Expert IK departmanına veya Expert IT departmanına bildiriniz." + "</h3>" +
                        "<p>inn: " + innerexp + "</p>" + "<p>" + source + "</p>", contentType: "text/html; charset=utf-8;");
                }
                return Content("HATA:" + ex.Message + "<br/>" + "<h3>Lütfen buradaki hata mesajlarını Expert IK departmanına veya Expert IT departmanına bildiriniz.</h3>",
                    contentType: "text/html; charset=utf-8;");
            }



        }
        [Route("configuration/get-station/{id}")]
        public Station GetStation(int id)
        {
            Thread.Sleep(250);
            return _context.Station.FirstOrDefault(i => i.Id == id)!;
        }
        [Route("configuration/get-control/{id}")]
        public Control GetControl(int id)
        {
            return _context.Control.FirstOrDefault(i => i.Id == id)!;
        }
        [Route("configuration/get-definition/{id}")]
        public Definition GetDefinition(int id)
        {
            var definition = _context.Definition.FirstOrDefault(i => i.Id == id)!;
            definition.Station = _context.Station.FirstOrDefault(i => i.Id == definition.StationId);
            definition.Control = _context.Control.FirstOrDefault(i => i.Id == definition.ControlId);
            //Thread.Sleep(1000);
            return definition;
        }
        [Route("configuration/upsert-station")]
        public async Task<ObjectVM> UpsertStation([FromBody] Station data)
        {
            try
            {
                var id = data.Id;
                if (data.Id > 0)
                    _context.Station.Update(data);
                else
                    _context.Station.Add(data);

                var status = await _context.SaveChangesAsync();
                return new ObjectVM
                {
                    Code = status > 0 ? 200 : 500,
                    Status = status > 0 ? "Başarılı!" : "Hata",
                    Data = data,
                    Message = id > 0 ? "İstasyon bilgisi başarılı bir şekilde güncelleştirildi." : "Yeni bir istasyon eklediniz."
                };
            }
            catch (Exception ex)
            {
                return new ObjectVM
                {
                    Code = 500,
                    Status = "error",
                    Data = data,
                    Message = ex.Message
                };
            }
        }
        [Route("configuration/del-station")]
        public async Task<ObjectVM> DeleteStation([FromBody] Station data)
        {
            try
            {
                var id = data.Id;
                data = _context.Station.Where(i => i.Id == id).FirstOrDefault()!;
                _context.Station.Remove(data);

                var status = await _context.SaveChangesAsync();
                return new ObjectVM
                {
                    Code = status > 0 ? 200 : 500,
                    Status = status > 0 ? "Başarılı!" : "Hata",
                    Data = data,
                    Message = "İstasyon yetkisi başarılı bir şekilde KALDIRILDI! Artık istemci istasyonu sunucuya erişemeyecek."
                };
            }
            catch (Exception ex)
            {
                return new ObjectVM
                {
                    Code = 500,
                    Status = "error",
                    Data = data,
                    Message = ex.Message
                };
            }
        }

        [Route("configuration/upsert-control")]
        public async Task<ObjectVM> UpsertControl([FromBody] Control data)
        {
            //return null;
            try
            {
                var id = data.Id;
                if (data.Id > 0)
                    _context.Control.Update(data);
                else
                {
                    data.Active = true;
                    _context.Control.Add(data);
                }

                var status = await _context.SaveChangesAsync();
                return new ObjectVM
                {
                    Code = status > 0 ? 200 : 500,
                    Status = status > 0 ? "Başarılı!" : "Hata",
                    Data = data,
                    Message = id > 0 ? "Kontrol bilgisi başarılı bir şekilde güncelleştirildi." : "Yeni bir kontrol bilgisi eklediniz."
                };
            }
            catch (Exception ex)
            {
                return new ObjectVM
                {
                    Code = 500,
                    Status = "error",
                    Data = data,
                    Message = ex.Message
                };
            }
        }
        [Route("configuration/definition/add-control")]
        public async Task<IActionResult> PostDefinitionControl(Definition data)
        {
            foreach (var item in data.ShiftList)
                data.Shift += item + "+";
            if (data.ShiftList.Count() > 0)
                data.Shift = data.Shift.Remove(data.Shift.Length - 1).ToUpper().ToString();

            try
            {
                if (data.ControlId == 0)
                    return Redirect("/configuration/definition/" + data.StationId.ToString() + "?message=" + HttpUtility.UrlEncode("Kontrol seçilmediği için kayıt yapılamadı."));
                var control = _context.Control.FirstOrDefault(i => i.Id == data.ControlId);
                var station = _context.Station.FirstOrDefault(i => i.Id == data.StationId);
                data.Station = station;
                data.Control = control;
                _context.Add(data);
                _context.SaveChanges();

                if (data.ImageFile != null && data.ImageFile.ContentType == "image/png")
                {
                    var IsSuccess = false;
                    data.Image = true;


                    var imagepath = Path.Combine(_env.WebRootPath, "assets", "frame-images");
                    var filePath = Path.Combine(imagepath, data.Id + ".png");

                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);

                    var timeout = TimeSpan.FromSeconds(2);
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await data.ImageFile.CopyToAsync(fileStream).WaitAsync(timeout);
                        IsSuccess = true;
                    }
                    Console.WriteLine("-------------------");
                    Console.WriteLine("Resim formatı uygun. Dosyayolu: " + filePath);

                    if (IsSuccess)
                    {
                        _context.Update(data);
                        _context.SaveChanges();
                        Console.WriteLine("Resim başarılı bir şekilde kopyalandı.");
                    }

                }
                else
                    Console.WriteLine("Resimsiz Upload yapılıyor.. [TEST]");

            }
            catch (Exception ex)
            {
                return Content("HATA:" + ex.Message + "<br/>" + "<h6>Lütfen bu hatayı IK departmanına veya IT departmanına bildiriniz." + "</h6>");
            }
            return Redirect("/configuration/definition/" + data.StationId+"/"+data.ComponentId);
        }
        [HttpPost("configuration/update-definitions")]
        public object UpdateDefinitions([FromBody] IEnumerable<Definition> dataList)
        {

            dataList = dataList.Where(i => i != null);
            //return null;
            try
            {

                foreach (var definition in dataList)
                {
                    var imagepath = Path.Combine(_env.WebRootPath, "assets", "frame-images");
                    var filePath = Path.Combine(imagepath, definition.Id + ".png");
                    //(data.image) backend e post olarak false göderilirse resmi dosyalardan kaldır!!!
                    if (definition.Base64!.Length > 10)
                    {
                        string base64Data = definition.Base64;
                        string base64Header = "data:image/png;base64,";

                        if (base64Data.StartsWith(base64Header))
                        {
                            base64Data = base64Data.Substring(base64Header.Length);
                        }


                        byte[] bytes = Convert.FromBase64String(base64Data);

                        if (System.IO.File.Exists(filePath))
                            System.IO.File.Delete(filePath);

                        var timeout = TimeSpan.FromSeconds(2);
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            fileStream.Write(bytes, 0, bytes.Length);
                            fileStream.Close();

                            //await data.ImageFile.CopyToAsync(fileStream).WaitAsync(timeout);
                        }
                        definition.Image = true;
                    }
                    else
                    {
                        if (!definition.Image)
                        {
                            if (System.IO.File.Exists(filePath))
                                System.IO.File.Delete(filePath);
                        }
                    }
                }
                _context.UpdateRange(dataList);
                var status = _context.SaveChanges();
                if (status > 0)
                    return StatusCode(200, dataList);
                return StatusCode(500, null);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }

        }

        [HttpPost("configuration/delete-definition")]
        public object DeleteDefinition([FromBody] Definition data)
        {
            if (data == null)
                return StatusCode(404, new ObjectVM
                {
                    Code = 404,
                    Data = null,
                    Status = "DeleteFail",
                    Message = "Kotrol listeden çıkartılamadı! Veri null olarak gönderildi."
                });
            _context.Definition.Remove(data);
            var status = _context.SaveChanges();
            if (status > 0)
                return StatusCode(200, data);
            return StatusCode(500, new ObjectVM
            {
                Code = 500,
                Data = null,
                Status = "DeleteFail",
                Message = "Kotrol listeden çıkartılamadı!"
            });


        }
        [HttpGet("/configuration/get-definition-controlDet/{controlId}")]
        public object GetControlDetail(int controlId)
        {
            return _context.Control.FirstOrDefault(x => x.Id == controlId)!.Text;
        }
    }
}
