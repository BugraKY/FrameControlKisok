<?php
ob_start();
// Hata raporlamayı etkinleştir
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

// CORS başlıklarını ayarla
header('Access-Control-Allow-Origin: *'); // Her kökene izin ver
header('Access-Control-Allow-Methods: GET, POST, OPTIONS'); // İzin verilen methodları belirt
header('Access-Control-Allow-Headers: Content-Type, Authorization'); // İzin verilen başlıkları belirt
header('Content-Type: application/json'); // İçerik tipini JSON olarak belirt

// Dosya yolu
$configFilePath = '/var/www/html/approot/assets/config.json';

// POST isteği kontrolü
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    // Gelen JSON verisini al
    $jsonString = file_get_contents('php://input');
    $data = json_decode($jsonString, true);

    // JSON hata kontrolü
    if (json_last_error() === JSON_ERROR_NONE) {
        // Dosyaya yaz ve başarılı yanıt gönder
        if (file_put_contents($configFilePath, $jsonString) !== false) {
            echo json_encode(["message" => "Config dosyası başarıyla oluşturuldu veya güncellendi."]);
            exit;
        } else {
            // Dosyaya yazma hatası
            echo json_encode(["error" => "Dosyaya yazılamadı. İzinleri kontrol edin."]);
            exit;
        }
    } else {
        // JSON hatalı
        echo json_encode(["error" => "Gönderilen veri geçerli bir JSON değil."]);
        exit;
    }
} else {
    // GET isteği kontrolü
    if (file_exists($configFilePath)) {
        // Mevcut dosyayı oku ve gönder
        echo file_get_contents($configFilePath);
        exit;
    } else {
        // Dosya yok, varsayılan konfigürasyonu oluştur
        $defaultConfig = [
            "stationLine" => 0,
            "stationName" => "none",
            "ip" => "0.0.0.0",
            "mac" => "00:00:00:00:00:00"
        ];
        // Dosyaya yaz ve varsayılan konfigürasyonu gönder
        if (file_put_contents($configFilePath, json_encode($defaultConfig)) !== false) {
            echo json_encode($defaultConfig);
            exit;
        } else {
            // Dosyaya yazma hatası
            echo json_encode(["error" => "Varsayılan dosya oluşturulamadı. İzinleri kontrol edin."]);
            exit;
        }
    }
}

ob_end_clean();
echo json_encode($data);
exit;
?>