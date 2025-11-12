window.showLocationWarningIfNeeded = function (warningId, timeoutMs = 2000) {
    setTimeout(function () {
        if (!(window.google && google.maps && google.maps.places)) {
            var el = document.getElementById(warningId);
            if (el) el.style.display = 'block';
        }
    }, timeoutMs);
};