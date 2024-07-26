using FrameControlKiosk.ViewModels;

namespace FrameControlKiosk.Extensions
{
    public class AppConstant
    {
        public static List<DetectedDevices>? DetectedDevices { get; set; }
        public static bool ServerIsReady { get; set; } = false;
        public static string redirectScript = @"
    <script src=""/lib/jquery/dist/jquery.min.js""></script>
        <script>
        document.addEventListener('DOMContentLoaded', function () {
            setTimeout(function() {
                window.location.href = 'http://127.0.0.1/';
            }, 5000);
        });
        </script>";

        public static string htmlContent = $"<h2>Bu istasyon doğrulanamıyor..</h2>{redirectScript}";
    }
}
