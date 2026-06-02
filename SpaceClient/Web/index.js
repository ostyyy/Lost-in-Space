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
    iconUrl: 'ISS.png',
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
}
