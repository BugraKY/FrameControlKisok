using FrameControlKiosk.Data;
using FrameControlKiosk.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;

namespace FrameControlKiosk.Extensions
{
    public class Services
    {
        private static WebSocket secondStationWebSocket;
        private static Dictionary<string, WebSocket> stationWebSockets = new Dictionary<string, WebSocket>();
        /*
        public static void DetecthDevices()
        {
            //AppConstant.DetectedDevices = GetArpResultsForInterface("0x8");

        }*/
        public static string NormalizeMacAddress(string macAddress)
        {
            return macAddress.Replace("-", ":").ToUpper();
        }
        public static void DetecthDevices(ApplicationDbContext dbContext)
        {
            try
            {
                var detectedDevices = GetArpResultsForInterface("0x8");//burası windows lara veya linuxlara göre değişebiliyor. 0x8   0xa
                AppConstant.DetectedDevices = detectedDevices;
                var stations = dbContext.Station.ToList();

                foreach (var device in detectedDevices)
                {
                    var isAllowed = stations.Any(station => station.IP == device.Ip && station.Mac == device.MacAddress);
                    if (isAllowed)
                    {
                        Console.WriteLine($"Access granted to device with IP: {device.Ip} and MAC: {device.MacAddress}");
                    }
                    /*
                    if (device.Static)
                    {
                    */
                }
                /*
                foreach (var station in stations)
                {
                    if (!PingDevice(station.IP))
                    {
                        AppConstant.DetectedDevices = GetArpResultsForInterface("0x8");

                        break; // İlk erişilemeyen cihazda döngüyü kırabilir veya devam ettirebilirsiniz.
                    }
                }*/
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }
        static List<DetectedDevices> GetArpResultsForInterface(string interfaceCode)
        {
            List<DetectedDevices> devices = new List<DetectedDevices>();

            ProcessStartInfo startInfo = new ProcessStartInfo("arp", "-a")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo)!)
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    bool isCurrentInterface = false;

                    foreach (var line in lines)
                    {
                        if (line.Contains(interfaceCode))
                        {
                            isCurrentInterface = true;
                            continue;
                        }

                        if (line.StartsWith("Interface:"))
                        {
                            isCurrentInterface = false;
                        }

                        if (isCurrentInterface)
                        {
                            var match = Regex.Match(line, @"(\d{1,3}(\.\d{1,3}){3})\s+([0-9a-f-]+)\s+(\w+)");
                            if (match.Success)
                            {
                                devices.Add(new DetectedDevices
                                {
                                    Ip = match.Groups[1].Value,
                                    MacAddress = match.Groups[3].Value,
                                    Static = match.Groups[4].Value.Equals("static", StringComparison.OrdinalIgnoreCase)
                                });
                            }
                        }
                    }
                }
            }
            return devices;
        }
        static bool PingDevice(string ipAddress)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(ipAddress, 1000); // 1000 milisaniye zaman aşımı
                    if (reply.Status == IPStatus.Success)
                        return true;
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Hata işleme kodları
                Console.WriteLine($"Ping işlemi sırasında hata oluştu: {ex.Message}");
                return false;
            }
        }
        /*
        public static async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Metin mesajını işle
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    // Entity Framework sorgularını burada yapabilirsiniz.
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
            }
        }*/
        /*
        public static async Task HandleWebSocketConnection(WebSocket webSocket, string stationId)
        {
            // Bağlantıyı ilgili istasyona göre işle
            if (stationId == "Station1")
            {
                // İlk istasyondan mesajları al ve ikinci istasyona yönlendir
            }
            else if (stationId == "Station2")
            {
                secondStationWebSocket = webSocket;
                // İkinci istasyon ile iletişimi burada işle
                
            }
            // WebSocket iletişimi işleme
        }
        */
        public static async Task HandleWebSocketConnection(WebSocket webSocket, string stationId)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;
            if (int.Parse(stationId) > 0)
                while (webSocket.State == WebSocketState.Open)
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        dynamic message = JsonConvert.DeserializeObject(receivedMessage);

                        if (message!.type == "processingDone")
                        {
                            // Mesajı ilgili istasyona gönder
                            await RouteMessageToNextStation(message.nextStation, receivedMessage);
                        }
                    }
                    // Diğer mesaj türlerini ve WebSocket durumunu işleme
                }
        }
        private static async Task RouteMessageToNextStation(string currentStationId, string message)
        {
            // Örneğin, mevcut istasyon "Station1" ise, mesajı "Station2"ye yönlendir
            var nextStationId = "Station" + (int.Parse(currentStationId.Replace("Station", "")) + 1).ToString();
            if (stationWebSockets.TryGetValue(nextStationId, out WebSocket nextStationWebSocket))
            {
                if (nextStationWebSocket.State == WebSocketState.Open)
                {
                    var buffer = Encoding.UTF8.GetBytes(message);
                    await nextStationWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
