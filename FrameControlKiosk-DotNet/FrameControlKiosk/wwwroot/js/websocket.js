var webSocket;

// WebSocket URL'sini dinamik olarak oluştur
var wsProtocol = window.location.protocol === "https:" ? "wss:" : "ws:";
var wsHost = window.location.host;
var wsPath = "/websocket"; // WebSocket endpoint yolu
var wsurl = wsProtocol + "//" + wsHost + wsPath;

// WebSocket bağlantısını başlat
var webSocket = new WebSocket(wsurl);

//var url = "ws://[SunucuAdresi]/path"; // Sunucunuzun WebSocket URL'si

/*
$(document).ready(function () {
    // WebSocket bağlantısını aç
 
});
*/

