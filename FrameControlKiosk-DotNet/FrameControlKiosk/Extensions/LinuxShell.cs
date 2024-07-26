using System.Diagnostics;

namespace FrameControlKiosk.Extensions
{
    public class LinuxShell
    {
        public static void OpenNautilus(string directory)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"nautilus '{directory}'\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
        }
        public static List<string> GetConnectedUSBDevicesAsync()
        {
            var devices = new List<string>();

            using (var process = new Process())
            {
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = "/opt/shell_scripts/usb_detector.sh";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    devices.Add(line);
                }

                process.WaitForExit();
            }

            return devices;
        }
    }
}
