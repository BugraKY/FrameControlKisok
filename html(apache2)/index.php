<!DOCTYPE html>
<html lang="tr">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=no">
    <title>ExpertFrameControlKiosk</title>
    <link rel="stylesheet" href="./approot/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="./approot/css/site.css" />
    <link rel="stylesheet" href="./approot/lib/font-awesome/css/all.min.css" />
    <link rel="icon" href="data:;base64,=">
    <style>

    </style>
</head>

<body>

    <div class="d-flex align-items-center justify-content-center" style="height:100%;">
        <div class="row" style="max-width:600px;">
            <div class="col-12 d-none" id="title_element">
                <h1 class="text-center">Sunucuya bağlan</h1>
            </div>

            <div class="col-12">
                <div class="alert alert-danger shadow d-none" id="connecting_fail">Sunucu bağlantısı başarısız!</div>
                <?php
                // Aktif ağ arayüzlerini ve ilgili MAC adreslerini al
                $output = shell_exec("ip link show | awk '/state UP/ {print $2}' | sed 's/://'");
                $interfaces = explode("\n", trim($output));
                $activeInterfacesMac = [];
                $macAddress = '';
                $i = 0;
                //echo "Aktif Arayüzlerin MAC Adresleri:\n";
                foreach ($interfaces as $interface) {
                    // Arayüz adından sonraki satırı almak için awk komutunu kullanın
                    $mac = shell_exec("ip link show $interface | awk '/link\\/ether/ {print $2}'");
                    $activeInterfacesMac[$interface] = trim($mac);
                    //echo $mac;
                    if ($interface != null) {
                        $i++;
                    }

                    $macAddress = $mac;

                }
                if ($i == 0) {
                    $macAddress = 'Bağlantı bulunamadığından mac adresi gösterilmiyor!';
                    echo "<div class='alert alert-warning shadow err-cls'>Cihaz ağa bağlı değil. Lütfen ethernet kablosunu veya çıkışları kontrol ediniz. Kablonun sağlıklı olduğuna emin olmalısınız.</div>";
                    sleep(3);
                    header('Location: http://127.0.0.1');
                }
                if ($i == 1) {

                }
                //print_r($activeInterfacesMac);
                




                // Eğer form gönderildiyse, JSON dosyasını güncelleyin
                if ($_SERVER["REQUEST_METHOD"] == "POST") {
                    ini_set('display_errors', 1);
                    ini_set('display_startup_errors', 1);
                    error_reporting(E_ALL);
                    // Formdan gelen verileri al
                    $stationLine = $_POST['stationLine'] ?? '';
                    $stationName = $_POST['stationName'] ?? '';
                    $ip = $_POST['ip'] ?? '';
                    $ip_server = $_POST['ip_server'] ?? '';
                    $port_server = $_POST['port_server'] ?? '';
                    $mac = $_POST['mac'] ?? '';

                    // Yeni verileri bir diziye yerleştir
                    $newData = [
                        'stationLine' => $stationLine,
                        'stationName' => $stationName,
                        'ip' => $ip,
                        'ip_server' => $ip_server,
                        'port_server' => $port_server,
                        'mac' => $mac,
                        'isServer' => false
                    ];

                 
                    $configFilePath = __DIR__ . '/approot/assets/config.json';

                    
                    $json = json_encode($newData, JSON_PRETTY_PRINT);

                 
                    if (file_put_contents($configFilePath, $json)) {
                        //echo "<div class='alert alert-warning shadow' id='fail_message'>Sunucuyla bağlantı yapılamadı.</div>";
                    } else {
                        echo "<div class='alert alert-warning shadow' id='fail_message'>Konfigürasyonu güncellerken bir hata oluştu.</div>";
                        ;
                    }
                }
                ?>


            </div>
            <div class="col-12">
                <form class="form-froup d-none" id="form_element" method="post"
                    action="<?php echo htmlspecialchars($_SERVER['PHP_SELF']); ?>">
                    <input class="form-control" type="number" placeholder="İstasyon Sırası" name="stationLine"
                        required />
                    <input class="form-control" type="text" placeholder="İstasyon Adı" name="stationName" required />
                    <input class="form-control" type="text" placeholder="Ip" name="ip" required />
                    <input class="form-control" type="text" placeholder="Server Ip" name="ip_server" required />
                    <input class="form-control" type="text" placeholder="Server Port" name="port_server" required />
                    
                    <input class="form-control" type="text" placeholder="Mac Adresi" name="mac"
                        value="<?php echo $macAddress; ?>" readonly />
                    <div>
                        <div class="alert alert-warning shadow d-none" id="validateip_alert">Lütfen geçerli bir Ip
                            adresi
                            giriniz.</div>
                    </div>
                    <div class="col text-center">
                        <input type="submit" class="btn btn-success d-none" id="submit_btn" value="Bağlan" />
                    </div>
                </form>
            </div>

            <div class="col-12 pt-2">
                <div class="alert alert-warning shadow" id="connecting_alert">Sunucuya bağlanıyor..</div>
            </div>

        </div>

        <div id="loader">
            <div class="d-flex align-items-center justify-content-center" style="min-height:100vh;">
                <img src="approot/assets/loader.png" class="loader-anim" width="50">
            </div>
        </div>

    </div>

    <div id="keyboard_text" class="keyboard_text shadow d-none">
        <div class="keyboard-header text-center">Klavye</div>
        <div class="row_keyboard">
            <div class="col-12" style="padding: 2px;">
                <button class="btn btn-outline-light" value="1"><span>1</span></button>
                <button class="btn btn-outline-light" value="2"><span>2</span></button>
                <button class="btn btn-outline-light" value="3"><span>3</span></button>
                <button class="btn btn-outline-light" value="4"><span>4</span></button>
                <button class="btn btn-outline-light" value="5"><span>5</span></button>
                <button class="btn btn-outline-light" value="6"><span>6</span></button>
                <button class="btn btn-outline-light" value="7"><span>7</span></button>
                <button class="btn btn-outline-light" value="8"><span>8</span></button>
                <button class="btn btn-outline-light" value="9"><span>9</span></button>
                <button class="btn btn-outline-light" value="0"><span>0</span></button>
                <button class="btn btn-outline-light" id="keyb_back" style="width:120px!important;">BackSpace</button>
            </div>
            <div class="col-12" style="padding: 2px;">
                <button class="btn btn-outline-light" value="q">q</button>
                <button class="btn btn-outline-light" value="w">w</button>
                <button class="btn btn-outline-light" value="e">e</button>
                <button class="btn btn-outline-light" value="r">r</button>
                <button class="btn btn-outline-light" value="t">t</button>
                <button class="btn btn-outline-light" value="y">y</button>
                <button class="btn btn-outline-light" value="u">u</button>
                <button class="btn btn-outline-light" value="ı">ı</button>
                <button class="btn btn-outline-light" value="o">o</button>
                <button class="btn btn-outline-light" value="p">p</button>
                <button class="btn btn-outline-light" value="ğ">ğ</button>
                <button class="btn btn-outline-light" value="ü">ü</button>
            </div>
            <div class="col-12" style="padding: 2px;">
                <button class="btn btn-outline-light" value="a">a</button>
                <button class="btn btn-outline-light" value="s">s</button>
                <button class="btn btn-outline-light" value="d">d</button>
                <button class="btn btn-outline-light" value="f">f</button>
                <button class="btn btn-outline-light" value="g">g</button>
                <button class="btn btn-outline-light" value="h">h</button>
                <button class="btn btn-outline-light" value="j">j</button>
                <button class="btn btn-outline-light" value="k">k</button>
                <button class="btn btn-outline-light" value="l">l</button>
                <button class="btn btn-outline-light" value="ş">ş</button>
                <button class="btn btn-outline-light" value="i">i</button>
                <button class="btn btn-outline-light" value=",">,</button>
            </div>
            <div class="col-12" style="padding: 2px;">
                <button class="btn btn-outline-light" value="z">z</button>
                <button class="btn btn-outline-light" value="x">x</button>
                <button class="btn btn-outline-light" value="c">c</button>
                <button class="btn btn-outline-light" value="v">v</button>
                <button class="btn btn-outline-light" value="b">b</button>
                <button class="btn btn-outline-light" value="n">n</button>
                <button class="btn btn-outline-light" value="m">m</button>
                <button class="btn btn-outline-light" value="ö">ö</button>
                <button class="btn btn-outline-light" value="ç">ç</button>
                <button class="btn btn-outline-light" value=".">.</button>
                <button class="btn btn-outline-light" id="keyb_shift" style="width:120px!important;">Shift</button>
            </div>
            <div class="col-12" style="padding: 2px;">
                <button class="btn btn-outline-light" value="Ø">Ø</button>
                <button class="btn btn-outline-light" value=""></button>
                <button class="btn btn-outline-light" value=""></button>
                <button class="btn btn-outline-light" id="keyb_space" style="width:50%;">Space</button>
                <button class="btn btn-outline-light" value="-">-</button>
                <button class="btn btn-outline-light" value="/">/</button>
                <button class="btn btn-outline-light" value="+">+</button>
            </div>


        </div>
    </div>
    <div id="keyboard_numeric" class="keyboard_numeric d-none">
        <div class="keyboard-header text-center">Numarik Klavye</div>
        <div class="row_keyboard">
            <div class="col-12">
                <button class="btn btn-outline-light" id="keyb_back" style="width:190px!important;">BackSpace</button>
            </div>
            <div class="col-12" style="padding: 2px;">
                <button class="btn btn-outline-light" value="7"><span>7</span></button>
                <button class="btn btn-outline-light" value="8"><span>8</span></button>
                <button class="btn btn-outline-light" value="9"><span>9</span></button>
            </div>
            <div class="col-12" style="padding: 2px;">
                <button class="btn btn-outline-light" value="4"><span>4</span></button>
                <button class="btn btn-outline-light" value="5"><span>5</span></button>
                <button class="btn btn-outline-light" value="6"><span>6</span></button>
            </div>
            <div class="col-12" style="padding: 2px;">
                <button class="btn btn-outline-light" value="1"><span>1</span></button>
                <button class="btn btn-outline-light" value="2"><span>2</span></button>
                <button class="btn btn-outline-light" value="3"><span>3</span></button>
            </div>
        </div>
    </div>


    <script type="text/javascript" src="./approot/lib/jquery/dist/jquery.min.js"></script>
    <script type="text/javascript" src="./approot/jqueryui/jquery-ui.min.js"></script>
    <script type="text/javascript" src="./approot/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script type="text/javascript" src="./approot/js/site.js"></script>
    <script>

        function validateIP(ip) {
            var ipRegex = /^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;
            return ipRegex.test(ip);
        }
        function validateMAC(mac) {
            var macRegex = /^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$/;
            return macRegex.test(mac);
        }


        $(document).ready(function () {
            // Global Ajax ayarlarında önbelleği devre dışı bırak
            $.ajaxSetup({ cache: false });
            textKeyboard_Funcs();
            numKeyboard_Funcs();
            // Dosya yolu
            var configFilePath = '/approot/assets/config.json?' + new Date().getTime();


            $.getJSON(configFilePath, function (data) {
                console.log(data);
                console.log(validateIP(data.ip));
                console.log(validateIP(data.ip_server));
                if (validateIP(data.ip) && validateIP(data.ip_server)) {

                    var jsonVal = {
                        'stationNum': data.stationLine,
                        'text': data.stationName,
                        'ip': data.ip,
                        'mac': data.mac,
                        'isServer': data.isServer
                    };


                    if ($('div.err-cls').length > 0) {
                        console.log("Bağlantı hatası");
                        $('#connecting_alert').addClass("d-none");
                        $('#loader').addClass("d-none");
                    }
                    else {
                        var url = "http://" + data.ip_server + ":" + data.port_server;
                        console.log("auth sending: " + url);
                        $.ajax({
                            url: url + "/auth",
                            method: "post",
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            data: JSON.stringify(jsonVal),
                            timeout: 10000,
                            beforeSend: function () {
                                $('#form_element').addClass("d-none");
                                $('#fail_message').addClass("d-none");
                                $('#connecting_fail').addClass("d-none");
                            },
                            success: function (ok) {
                                if (ok)
                                    window.location.href = url + "/station/" + data.stationLine;
                                else {
                                    $('#connecting_alert').text("istemci bilgileri doğru değil. Lütfen bilgileri kontrol ediniz.")
                                    $('#connecting_alert').removeClass("d-none");
                                    $('#loader').addClass("d-none");
                                    $('#title_element').removeClass("d-none");
                                    $('#form_element').removeClass("d-none");
                                    $('#submit_btn').removeClass("d-none");
                                }


                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                //if (jqXHR.status = 404)


                                $('#form_element').removeClass("d-none");
                                $('#submit_btn').removeClass("d-none");
                                $('#title_element').removeClass("d-none");
                                $('#connecting_alert').addClass("d-none");
                                $('#loader').addClass("d-none");
                                $('#connecting_fail').removeClass("d-none");
                                $('#fail_message').removeClass("d-none");
                                console.log(jqXHR);
                                console.log(textStatus);
                                console.log(errorThrown);
                            }
                        });
                    }
                }
                else {
                    $('#validateip_alert').removeClass("d-none");
                }

                $('input[name="stationLine"]').val(data.stationLine);
                $("input[name='stationName']").val(data.stationName);
                $("input[name='ip']").val(data.ip);
                $("input[name='ip_server']").val(data.ip_server);
                $("input[name='port_server']").val(data.port_server);
                //$("input[name='mac']").val(data.mac);

            }).fail(function () {
                console.log("An error has occurred.");
            });


        });

    </script>
</body>

</html>








<!-- nano /var/www/html/.htaccess bunu ekle; RewriteEngine On RewriteRule ^approot/php/checkjson\.php$ - [L]
    ---------------------------------------------------- sudo nano /etc/apache2/apache2.conf bunu ekle; <Directory
    /var/www/html>
    AllowOverride All
    </Directory>


    sudo apt update
    sudo apt install php libapache2-mod-php

    sudo a2enmod php7.4 # Yüklediğiniz PHP sürümüne göre değiştirin
    sudo service apache2 restart
    php -v


    sudo a2enmod rewrite
    sudo systemctl restart apache2





    -->