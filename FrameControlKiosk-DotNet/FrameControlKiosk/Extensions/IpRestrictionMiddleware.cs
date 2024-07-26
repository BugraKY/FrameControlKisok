using FrameControlKiosk.Data;

public class IpRestrictionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpContent _content;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _ip;

    public IpRestrictionMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, string ip)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _ip = ip;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var allowedIps = dbContext.Station.Select(i => i.IP).ToList();
            allowedIps.Add(_ip);
            allowedIps.Add("192.168.0.100");
            allowedIps.Add("192.168.0.110");
            allowedIps.Add("192.168.0.106");
            allowedIps.Add("127.0.0.1");
            allowedIps = allowedIps.OrderBy(x => x).ToList();
            var remoteIp = context.Connection.RemoteIpAddress;
            if (remoteIp != null && remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }
            var remoteIpString = remoteIp?.ToString();


            if (allowedIps.Contains(remoteIpString!)|| remoteIpString == "::1")
            {
                await _next(context);
            }
            else
            {
                context.Response.ContentType = "text/html";
                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                //var htmlContent = @"";

                await context.Response.WriteAsync(@"
<html lang='tr'>
<head>
    <meta charset='UTF-8'>
    <style>
        html, body {
            height: 100%;
            margin: 0;
            display: flex;
            justify-content: center;
            align-items: center;
            text-align: center;
        }
        .container {
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
        }
        .button {
            display: inline-block;
            padding: 10px 20px;
            font-size: 14px;
            cursor: pointer;
            text-align: center;
            text-decoration: none;
            outline: none;
            color: #fff;
            background-color: #4CAF50;
            border: none;
            border-radius: 15px;
            box-shadow: 0 9px #999;
        }
        .button:hover {background-color: #3e8e41}
        .button:active {
            background-color: #3e8e41;
            box-shadow: 0 5px #666;
            transform: translateY(4px);
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>Erişim Engellendi</h1>
        <p>Üzgünüz, bu IP adresinden erişiminize izin verilmiyor. IP adresiniz: " + remoteIpString + @"</p>
        <a href='/' class='button'>Yetkili sayfasını aç</a>
    <a href='http://127.0.0.1' class='button' style='margin-top:25px;'>Sunucuya bağlan</a>
    </div>
</body>
</html>");
                //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }
    }
}
