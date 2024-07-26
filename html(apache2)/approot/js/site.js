// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
/*
var $keyboard = $('#keyboard_text');
function changeLayout(newLayout) {

    $keyboard.empty();

    $.each(newLayout, function (_, key) {
        $('<button>').text(key)
            .appendTo($keyboard)
            .on('click', function () {
                var $input = $(':focus');
                $input.val($input.val() + key);
            });
    });
}
*/
function getCurrentUrl() {
    return $(location).attr('href');
}
function getPathName() {
    return $(location).attr('pathname');
}

$('#changeLayoutBtn').on('click', function () {
    //var newLayout = ['a', 's', 'd', 'f', 'g'];
    //changeLayout(newLayout);
});
/*
$('input[type=text]').on('mousedown', function (event) {

    console.log("Fokuslandı.");
});
*/
function textKeyboard_Funcs() {
    var $lastFocusedInput;
    // Sürüklenebilir Klavye
    $('#keyboard_text').draggable();

    // Klavye tuşlarına tıklandığında işlem

    $('#keyboard_text').on('mousedown', function (event) {
        event.preventDefault();
    });
    $('#keyboard_text').on('click', 'button', function (e) {
        e.stopPropagation();
        e.preventDefault();
        var key_value = $(this).val();
        var $focusedInput = $(':focus');

        if ($focusedInput.is('input[type="text"]') || $focusedInput.is('input[type="search"]') || $focusedInput.is('textarea.form-control')) {
            $lastFocusedInput = $focusedInput;
            // Shift ve BackSpace tuşları için özel işlemler
            if ($(this).attr('id') === 'keyb_shift') {
                $('#keyboard_text').toggleClass('shift-active');
                toggleShift(); // Shift tuşunun durumunu değiştir
            } else if ($(this).attr('id') === 'keyb_back') {

                var currentVal = $focusedInput.val();
                var caretPos = getCaretPosition($focusedInput[0]);

                // Caret'ın 0'dan büyük bir konumda olduğundan emin ol ve bir karakter sil
                if (caretPos > 0) {
                    var newVal = currentVal.substring(0, caretPos - 1) + currentVal.substring(caretPos);
                    $focusedInput.val(newVal);

                    // Caret konumunu bir geri al
                    setCaretPosition($focusedInput[0], caretPos - 1);
                }

                /*
                var currentVal = $focusedInput.val();
                $focusedInput.val(currentVal.substr(0, currentVal.length - 1));*/
            }
            else if ($(this).attr('id') === 'keyb_space') {

                var currentVal = $focusedInput.val();
                var caretPos = getCaretPosition($focusedInput[0]);

                // Caret konumuna boşluk eklemek
                var newVal = currentVal.substring(0, caretPos) + ' ' + currentVal.substring(caretPos);
                $focusedInput.val(newVal);

                // Caret konumunu boşluk karakterinin sonrasına taşı
                setCaretPosition($focusedInput[0], caretPos + 1);

                /*
                var currentVal = $focusedInput.val();
                $focusedInput.val($focusedInput.val() + ' ');
                */
            }
            else {


                var caretPos = getCaretPosition($focusedInput[0]);
                var currentVal = $focusedInput.val();

                // Shift durumuna göre karakteri caret konumuna ekle
                if ($('#keyboard_text').hasClass('shift-active')) {
                    key_value = key_value.toUpperCase();
                }

                var newVal = currentVal.substring(0, caretPos) + key_value + currentVal.substring(caretPos);
                $focusedInput.val(newVal);

                // Caret konumunu yeni karakterin sonrasına taşı
                setCaretPosition($focusedInput[0], caretPos + key_value.length);


                // Shift durumuna göre karakteri ekle
                /*
                if ($('#keyboard_text').hasClass('shift-active')) {
                    key_value = key_value.toUpperCase();
                }
                $focusedInput.val($focusedInput.val() + key_value);*/
            }
        }
        $('#keyboard_text').css('opacity', '0.8');
        $(this).blur();
        // Odağı geri al
        if ($lastFocusedInput && $lastFocusedInput.is('input[type="search"]')) {
            setTimeout(function () {
                $($lastFocusedInput).focus();
                $focusedInput.trigger('input');  // Bu satırı ekleyin
            }, 0); // Timeout kullanarak odağın hemen geri verilmesini sağla
        }

        //e.preventDefault();
    });
    $('#keyboard_text').on('click', function (e) {
        if ($lastFocusedInput != null)
            $lastFocusedInput.focus();
        $(this).blur();
        e.preventDefault();
    });
    $('#keyboard_text button').on('mousedown', function (event) {
        if ($lastFocusedInput != null)
            $lastFocusedInput.focus();
        $(this).blur();
        event.preventDefault();
    });
    // İnput alanlarına odaklanınca klavyeyi göster
    $(document).on('focus', 'input[type="text"],textarea.form-control', function (e) {
        $('#keyboard_text').removeClass('d-none');
        if ($lastFocusedInput == e.currentTarget)
            $('#keyboard_text').css('opacity', '1');
        $lastFocusedInput = e.currentTarget;
    });


    $(document).on('select2:opening', function (e) {
        if ($('#keyboard_text').css('display') !== 'none') {
            e.preventDefault();
        }
    });

    // Select2 açıldığında
    $(document).on('select2:open', () => {
        // Not: setTimeout kullanmak, Select2'nin arama kutusunun tamamen yüklenmesini beklemek içindir.
        setTimeout(() => {
            // Arama kutusunu bul ve odağı ayarla
            let searchBox = $('.select2-container--open .select2-search__field');
            if (searchBox.length && $lastFocusedInput !== searchBox[0]) {
                $lastFocusedInput = searchBox[0];
                $('#keyboard_text').removeClass('d-none').css('opacity', '1');
                $($lastFocusedInput).focus();
            }
        }, 100);
    });

    // Select2 kapatıldığında
    $(document).on('select2:close', () => {
        $('#keyboard_text').addClass('d-none').removeClass('shift-active');
        $lastFocusedInput = null;
    });
    $(document).on('select2:closing', function (e) {
        // Select2 container'ını bul
        var $select2Container = $(e.target).closest('.select2-container');
        // İlgili <select> elementini bul
        var $originalSelect = $select2Container.prev('select');

        // İlgili Select2'yi kapat
        $originalSelect.select2('close');

        // Gerekli işlemler
        if ($('#keyboard_text').is(':visible')) {
            e.preventDefault();
            setTimeout(function () {
                if ($lastFocusedInput) {
                    $($lastFocusedInput).focus();
                }
            }, 0);
        }
    });


    // İnput dışına tıklandığında klavyeyi gizle
    $(document).on('click', function (e) {
        if (!$(e.target).is('input[type="text"],textarea.form-control, #keyboard_text, #keyboard_text *')) {
            $('#keyboard_text').addClass('d-none').removeClass('shift-active');
            $lastFocusedInput = null;
        }
    });
}


function toTurkishUpperCase(str) {
    return str.replace(/i/g, 'İ').replace(/ı/g, 'I').toUpperCase();
}

function toTurkishLowerCase(str) {
    return str.replace(/İ/g, 'i').replace(/I/g, 'ı').toLowerCase();
}
function numKeyboard_Funcs() {
    var $lastFocusedInput;
    // Sürüklenebilir Klavye
    $('#keyboard_numeric').draggable();

    // Klavye tuşlarına tıklandığında işlem

    $('#keyboard_numeric').on('mousedown', function (event) {
        event.preventDefault();
    });
    $('#keyboard_numeric').on('click', 'button', function (e) {
        var key_value = $(this).val();
        var $focusedInput = $(':focus');
        if ($focusedInput.is('input[type="number"]')) {
            $lastFocusedInput = $focusedInput;
            // Shift ve BackSpace tuşları için özel işlemler
            if ($(this).attr('id') === 'keyb_shift') {
                $('#keyboard_numeric').toggleClass('shift-active');
                toggleShift(); // Shift tuşunun durumunu değiştir
            } else if ($(this).attr('id') === 'keyb_back') {

                
                var currentVal = $focusedInput.val();
                $focusedInput.val(currentVal.substr(0, currentVal.length - 1));


            }
            else if ($(this).attr('id') === 'keyb_space') {
                
                var currentVal = $focusedInput.val();
                $focusedInput.val($focusedInput.val() + ' ');
            }
            else {

                // Shift durumuna göre karakteri ekle

                if ($('#keyboard_numeric').hasClass('shift-active')) {
                    key_value = key_value.toUpperCase();
                }
                $focusedInput.val($focusedInput.val() + key_value);
            }
        }
        $('#keyboard_numeric').css('opacity', '0.8');
        $(this).blur();
        e.preventDefault();
    });
    $('#keyboard_numeric').on('click', function (e) {
        if ($lastFocusedInput != null)
            $lastFocusedInput.focus();
        $(this).blur();
        e.preventDefault();
    });
    $('#keyboard_numeric button').on('mousedown', function (event) {
        if ($lastFocusedInput != null)
            $lastFocusedInput.focus();
        $(this).blur();
        event.preventDefault();
    });
    // İnput alanlarına odaklanınca klavyeyi göster
    $(document).on('focus', 'input[type="number"]', function (e) {
        $('#keyboard_numeric').removeClass('d-none');
        if ($lastFocusedInput == e.currentTarget)
            $('#keyboard_numeric').css('opacity', '1');
        $lastFocusedInput = e.currentTarget;
    });

    // İnput dışına tıklandığında klavyeyi gizle
    $(document).on('click', function (e) {
        if (!$(e.target).is('input[type="number"], #keyboard_numeric, #keyboard_numeric *')) {
            $('#keyboard_numeric').addClass('d-none').removeClass('shift-active');
            $lastFocusedInput = null;
        }
    });
}

function reportTouchEvent(e) {
    //window.webkit.messageHandlers.touchEvent.postMessage(e.type);
    window.webkit.messageHandlers.observe.postMessage(e.type);
    console.log(e);

}

function getCaretPosition(input) {
    if (!input) return 0;
    if ('selectionStart' in input) {
        // input ve textarea için
        return input.selectionStart;
    } else if (document.selection) {
        // IE için
        input.focus();
        var sel = document.selection.createRange();
        var selLength = document.selection.createRange().text.length;
        sel.moveStart('character', -input.value.length);
        return sel.text.length - selLength;
    }
}

function setCaretPosition(elem, caretPos) {
    if (elem !== null) {
        if (elem.createTextRange) {
            var range = elem.createTextRange();
            range.move('character', caretPos);
            range.select();
        } else {
            if (elem.selectionStart) {
                elem.focus();
                elem.setSelectionRange(caretPos, caretPos);
            } else
                elem.focus();
        }
    }
    /*
    if (!input) return;
    if (input.setSelectionRange) {
        // input ve textarea için
        input.focus();
        input.setSelectionRange(pos, pos);
    } else if (input.createTextRange) {
        // IE için
        var range = input.createTextRange();
        range.collapse(true);
        range.moveEnd('character', pos);
        range.moveStart('character', pos);
        range.select();
    }
    */
}
function toggleShift() {
    $('#keyboard_text button').each(function () {
        var isShiftActive = $('#keyboard_text').hasClass('shift-active');
        var value = $(this).val();

        if (value) {
            $(this).val(isShiftActive ? toTurkishUpperCase(value) : toTurkishLowerCase(value));
            $(this).text(isShiftActive ? toTurkishUpperCase(value) : toTurkishLowerCase(value));
        }
    });
}

function getTouchPos(element, touchEvent) {
    var rect = element.getBoundingClientRect();
    var touch = touchEvent.touches[0];
    return {
        x: touch.clientX - rect.left,
        y: touch.clientY - rect.top
    };
}

function getCaretIndexFromTouch(input, touchX) {
    // Bu fonksiyon, dokunulan x konumunu alır ve imleç indeksini döndürür.
    // Gerçek uygulamada daha karmaşık hesaplamalar gerekebilir.
    var inputStyle = window.getComputedStyle(input);
    var paddingLeft = parseFloat(inputStyle.paddingLeft);
    var paddingRight = parseFloat(inputStyle.paddingRight);
    var totalWidth = input.offsetWidth - paddingLeft - paddingRight;
    var charWidth = totalWidth / input.value.length;
    return Math.floor((touchX - paddingLeft) / charWidth);
}

document.addEventListener('touchstart', function (e) {
    if (e.touches.length > 1) {
        e.preventDefault();
    }
}, { passive: false });
$(document).on('touchstart touchmove', 'input[type="text"],input[type="number"],textarea.form-control', function (e) {
    var touchPos = getTouchPos(this, e.originalEvent);
    var caretIndex = getCaretIndexFromTouch(this, touchPos.x);
    setCaretPosition(this, caretIndex);
    e.preventDefault(); // Varsayılan olayı engeller (örneğin, sayfa kaydırma)
});
/*
document.addEventListener('touchmove', e => {
    if (e.touches.length > 1) {
        e.preventDefault();
    }
}, { passive: false });
*/



function disableCtrlKeyCombination(e) {

    //list all CTRL + key combinations you want to disable
    var forbiddenKeys = new Array('a', 'n', 'c', 'x', 'v', 'j', 'w');
    var key;
    var isCtrl;
    if (window.event) {
        key = window.event.keyCode; //IE
        if (window.event.ctrlKey)
            isCtrl = true;
        else
            isCtrl = false;
    }
    else {
        key = e.which; //firefox
        if (e.ctrlKey)
            isCtrl = true;
        else
            isCtrl = false;
    }
    //if ctrl is pressed check if other key is in forbidenKeys array
    /*
    if (isCtrl) {
        for (i = 0; i < forbiddenKeys.length; i++) {
            //case-insensitive comparation
            if (forbiddenKeys[i].toLowerCase() == String.fromCharCode(key).toLowerCase()) {
                alert('CTRL + ' + String.fromCharCode(key) + 'tuş kombinasyonuna izin verilmiyor.');
                return false;
            }
        }
    }*/
    return true;
};


function testNoSelectText() {
    // input ve textarea elementlerini seç
    var inputs = document.querySelectorAll('input[type="text"], textarea');
    console.log(inputs);
    // Her input/textarea elementi için olay dinleyicileri ekle
    inputs.forEach(function (input) {
        console.log(input);
        // Seçimi engelle
        input.classList.add('no-select');

        // Klavye olaylarını yakala
        input.addEventListener('keydown', function (e) {
            // Seçim izni verilen tuşların listesi
            const allowedKeys = [
                37, // ArrowLeft
                38, // ArrowUp
                39, // ArrowRight
                40, // ArrowDown
                8,  // Backspace
                46  // Delete
                // Buraya yazım ve diğer izin verilen tuşları ekleyebilirsiniz
            ];

            // Eğer basılan tuş izin verilenler listesinde değilse olayı engelle
            if (!allowedKeys.includes(e.keyCode)) {
                e.preventDefault();
            }
        });

        // Fare ile seçimi engelle
        input.addEventListener('mousedown', function (e) {
            e.preventDefault();
        });
    });
};


function refrPage() {
    //location.reload();//$(location).attr('href');
    window.location.replace(getPathName() + '?toastMessage=tanımlama+alanı+yeniden%20yüklendi!');
    //console.log(getPathName() + '?toastMessage=tanımlama+alanı+yeniden%20yüklendi!');

};