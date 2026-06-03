alert("Скрипт успешно обновился! Загружаем орбиту...");

var map = L.map('map').setView([0, 0], 3);

L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
    attribution: '&copy; OpenStreetMap contributors &copy; CARTO',
    maxZoom: 10
}).addTo(map);

var issIcon = L.divIcon({ className: 'iss-icon', iconSize: [40, 40], iconAnchor: [20, 20] });
var issMarker = L.marker([0, 0], { icon: issIcon }).addTo(map);

// Создаем линию будущей орбиты
var orbitPath = L.polyline([], {
    color: '#FD71CB', // <-- Убрали лишние FF в начале!
    weight: 3,
    dashArray: '10, 10',
    opacity: 0.8
}).addTo(map);

// --- РАСЧЕТ БУДУЩЕГО ПУТИ ---
async function drawFutureOrbit() {
    try {
        // 1. Скачиваем TLE (математическую модель орбиты МКС)
        const response = await fetch('https://api.wheretheiss.at/v1/satellites/25544/tles');
        const data = await response.json();

        // 2. Инициализируем расчетчик
        const satrec = satellite.twoline2satrec(data.line1, data.line2);
        const latlngs = [];
        const now = new Date();

        // 3. Просчитываем 90 точек (по 1 точке на каждую минуту вперед = 1 виток вокруг Земли)
        for (let i = 0; i <= 90; i++) {
            const futureTime = new Date(now.getTime() + i * 60000);
            const positionAndVelocity = satellite.propagate(satrec, futureTime);
            const gmst = satellite.gstime(futureTime);
            const positionGd = satellite.eciToGeodetic(positionAndVelocity.position, gmst);

            const lat = satellite.degreesLat(positionGd.latitude);
            const lng = satellite.degreesLong(positionGd.longitude);
            latlngs.push([lat, lng]);
        }

        // 4. Защита от перелома карты на антимеридиане
        const correctedPaths = [];
        let currentSegment = [];

        for (let i = 0; i < latlngs.length; i++) {
            currentSegment.push(latlngs[i]);
            // Если разница по долготе между точками огромна (>180) - мы пересекаем границу карты. Режем линию!
            if (i < latlngs.length - 1 && Math.abs(latlngs[i][1] - latlngs[i + 1][1]) > 180) {
                correctedPaths.push(currentSegment);
                currentSegment = [];
            }
        }
        correctedPaths.push(currentSegment);

        // 5. Рисуем траекторию!
        orbitPath.setLatLngs(correctedPaths);

        console.log("Орбита успешно просчитана! Количество точек:", latlngs.length);

    } catch (error) {
        console.error("Ошибка расчета орбиты:", error);
    }
}

// Запускаем отрисовку орбиты сразу при загрузке
drawFutureOrbit();
// И перерисовываем орбиту каждые 15 минут (так как Земля вращается под орбитой)
setInterval(drawFutureOrbit, 900000);


// --- ОБНОВЛЕНИЕ ТЕКУЩЕЙ ПОЗИЦИИ ИЗ C# ---
// Эту функцию вызывает твой WPF (MyWebView.ExecuteScriptAsync) каждые 3 секунды
function updateISS(lat, lon) {
    issMarker.setLatLng([lat, lon]);
    map.setView([lat, lon], map.getZoom(), { animate: true });
}