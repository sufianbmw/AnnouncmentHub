document.addEventListener("DOMContentLoaded", function () {

    var map = L.map("map").setView([existingLat, existingLng], hasExisting ? 15 : 10);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "© OpenStreetMap contributors"
    }).addTo(map);

    var marker = null;

    if (hasExisting) {
        marker = L.marker([existingLat, existingLng], { draggable: true }).addTo(map);
        marker.on("dragend", function (e) {
            setCoords(e.target.getLatLng().lat, e.target.getLatLng().lng);
        });
    }

    map.on("click", function (e) {
        placeMarker(e.latlng.lat, e.latlng.lng);
    });

    function placeMarker(lat, lng) {
        if (marker) map.removeLayer(marker);
        marker = L.marker([lat, lng], { draggable: true }).addTo(map);
        marker.on("dragend", function (e) {
            setCoords(e.target.getLatLng().lat, e.target.getLatLng().lng);
        });
        setCoords(lat, lng);
    }

    function setCoords(lat, lng) {
        document.getElementById("Latitude").value = parseFloat(lat).toFixed(6);
        document.getElementById("Longitude").value = parseFloat(lng).toFixed(6);
    }

    window.getCurrentLocation = function () {
        if (!navigator.geolocation) {
            alert("المتصفح لا يدعم تحديد الموقع.");
            return;
        }
        navigator.geolocation.getCurrentPosition(function (pos) {
            map.setView([pos.coords.latitude, pos.coords.longitude], 16);
            placeMarker(pos.coords.latitude, pos.coords.longitude);
        }, function () {
            alert("تعذر تحديد موقعك. تأكد من منح الإذن للمتصفح.");
        });
    };

    window.resetMap = function () {
        if (marker) map.removeLayer(marker);
        marker = null;
        document.getElementById("Latitude").value = "";
        document.getElementById("Longitude").value = "";
    };

    window.extractFromLink = function () {
        var input = document.getElementById("locationLinkInput").value.trim();
        var errorDiv = document.getElementById("linkError");
        errorDiv.style.display = "none";

        var lat = null, lng = null, match;

        match = input.match(/[?&]q=(-?\d+\.?\d*),(-?\d+\.?\d*)/);
        if (match) { lat = match[1]; lng = match[2]; }

        if (!lat) {
            match = input.match(/@(-?\d+\.?\d*),(-?\d+\.?\d*)/);
            if (match) { lat = match[1]; lng = match[2]; }
        }

        if (!lat) {
            match = input.match(/\/place\/[^/]*\/@(-?\d+\.?\d*),(-?\d+\.?\d*)/);
            if (match) { lat = match[1]; lng = match[2]; }
        }

        if (!lat) {
            match = input.match(/maps\?q=(-?\d+\.?\d*),(-?\d+\.?\d*)/);
            if (match) { lat = match[1]; lng = match[2]; }
        }

        if (!lat) {
            match = input.match(/^(-?\d+\.?\d*)\s*,\s*(-?\d+\.?\d*)$/);
            if (match) { lat = match[1]; lng = match[2]; }
        }

        if (lat && lng) {
            placeMarker(parseFloat(lat), parseFloat(lng));
            map.setView([parseFloat(lat), parseFloat(lng)], 16);
            document.getElementById("locationLinkInput").value = "";
        } else {
            errorDiv.textContent = (input.includes("goo.gl") || input.includes("maps.app"))
                ? "⏳ الرابط مختصر، افتحه في المتصفح وانسخ الرابط الكامل."
                : "⚠️ تعذر الاستخراج. جرب لصق الإحداثيات مثل: 31.9539, 35.9106";
            errorDiv.style.display = "block";
        }
    };
});