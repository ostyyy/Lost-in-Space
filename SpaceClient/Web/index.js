const map = L.map('map', { zoomControl: false }).setView([0, 0], 2);

// Темна тема за замовчуванням
const darkTheme = L.tileLayer(
    'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',
    {
        attribution: '&copy; OpenStreetMap &copy; CARTO',
    },
).addTo(map);

const satelliteTheme = L.tileLayer(
    'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
    {
        attribution: '&copy; Esri',
    },
);

const issIcon = L.icon({
    iconUrl:
        'https://static.isstracker.pl/images/satellites_icon/4/44/iss-25544-v2.png',
    iconSize: [60, 60],
    iconAnchor: [30, 30],
});

const marker = L.marker([0, 0], { icon: issIcon }).addTo(map);

const radarCircle = L.circleMarker([0, 0], {
    radius: 100,
    className: 'radar-glow',
}).addTo(map);

let isSatellite = false;
function toggleMapMode() {
    if (!isSatellite) {
        map.removeLayer(darkTheme);
        satelliteTheme.addTo(map);
    } else {
        map.removeLayer(satelliteTheme);
        darkTheme.addTo(map);
    }
    isSatellite = !isSatellite;
}

function focusOnIss() {
    const currentPos = marker.getLatLng();
    map.flyTo(currentPos, map.getZoom(), {
        animate: true,
        duration: 1.2,
    });
}

function updateISS(lat, lng) {
    const newPos = [lat, lng];
    marker.setLatLng(newPos);
    radarCircle.setLatLng(newPos);
    //map.setView(newPos, map.getZoom());
}

const orbitPath = L.polyline([], {
    color: '#FD71CB',
    weight: 3,
    dashArray: '10, 10',
    opacity: 0.8
}).addTo(map);

async function drawFutureOrbit() {
    try {
        const response = await fetch('https://api.wheretheiss.at/v1/satellites/25544/tles');
        const data = await response.json();

        if (typeof satellite === 'undefined') {
            console.error("КРИТИЧЕСКАЯ ОШИБКА: Библиотека satellite.js не найдена!");
            return;
        }

        const satrec = satellite.twoline2satrec(data.line1, data.line2);
        const latlngs = [];
        const now = new Date();

        for (let i = 0; i <= 90; i++) {
            const futureTime = new Date(now.getTime() + i * 60000);
            const positionAndVelocity = satellite.propagate(satrec, futureTime);
            const gmst = satellite.gstime(futureTime);
            const positionGd = satellite.eciToGeodetic(positionAndVelocity.position, gmst);

            const lat = satellite.degreesLat(positionGd.latitude);
            const lng = satellite.degreesLong(positionGd.longitude);
            latlngs.push([lat, lng]);
        }

        const correctedPaths = [];
        let currentSegment = [];

        for (let i = 0; i < latlngs.length; i++) {
            currentSegment.push(latlngs[i]);
            if (i < latlngs.length - 1 && Math.abs(latlngs[i][1] - latlngs[i + 1][1]) > 180) {
                correctedPaths.push(currentSegment);
                currentSegment = [];
            }
        }
        correctedPaths.push(currentSegment);

        orbitPath.setLatLngs(correctedPaths);

    } catch (error) {
        console.error("Error:", error);
    }
}

drawFutureOrbit();
setInterval(drawFutureOrbit, 900000);