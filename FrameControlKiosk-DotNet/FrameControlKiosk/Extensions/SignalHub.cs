using Microsoft.AspNetCore.SignalR;

namespace FrameControlKiosk.Extensions
{
    public class SignalHub : Hub
    {
        public async Task SendMessage(string stationLine, string message)
        {
            var calcdstationLine = int.Parse(stationLine) + 1;
            await Clients.All.SendAsync("ReceiveMessage", calcdstationLine.ToString(), message);
        }
    }
}
