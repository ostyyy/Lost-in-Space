const map = L.map('map', { zoomControl: false }).setView([0, 0], 2);

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

// ICONS
const issIcon = L.icon({
    iconUrl:
        'https://static.isstracker.pl/images/satellites_icon/4/44/iss-25544-v2.png',
    iconSize: [60, 60],
    iconAnchor: [30, 30],
});

const sunIcon = L.icon({
    iconUrl: 'https://static.isstracker.pl/img/layout/icon_sun.png',
    iconSize: [50, 50],
    iconAnchor: [20, 20],
});

const moonIcon = L.icon({
    iconUrl: 'https://cdn.pixabay.com/photo/2014/04/02/17/07/full-moon-308007_640.png',
    iconSize: [30, 30],
    iconAnchor: [20, 20],
})


// ISS
const marker = L.marker([0, 0], { icon: issIcon }).addTo(map);

const radarCircle = L.circleMarker([0, 0], {
    radius: 100,
    className: 'radar-glow',
}).addTo(map);

// SUN
const sunMarker = L.marker([0, 0], { icon: sunIcon }).addTo(map);
const moonMarker = L.marker([0, 0], { icon: moonIcon }).addTo(map);

const sunIconImage = sunMarker.getElement();
if (sunIconImage) {
    sunIconImage.classList.add('sun-glow');
}

// MAP
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

// SUN POSITION
function updateSunPosition() {
    const now = new Date();

    const sunPos = SunCalc.getPosition(now, 0, 0);

    const lat = sunPos.altitude * (180 / Math.PI);

    const utcHours =
        now.getUTCHours() +
        now.getUTCMinutes() / 60 +
        now.getUTCSeconds() / 3600;
    let lng = 180 - utcHours * 15;
    if (lng < -180) lng += 360;
    if (lng > 180) lng -= 360;

    sunMarker.setLatLng([lat, lng]);
}

function updateMoonPosition()
{
    const now = new Date();
    const moonPos = SunCalc.getMoonPosition(now, 0, 0);
    const lat = moonPos.altitude * (180 / Math.PI);
    const utcHours =
        now.getUTCHours() +
        now.getUTCMinutes() / 60 +
        now.getUTCSeconds() / 3600;
    let lng = 180 - (utcHours * 15) - (moonPos.azimuth * (180 / Math.PI));
    if (lng < -180) lng += 360;
    if (lng > 180) lng -= 360;

    moonMarker.setLatLng([lat, lng]);
}

// ISS TRACKING
function updateISS(lat, lng) {
    const newPos = [lat, lng];
    marker.setLatLng(newPos);
    radarCircle.setLatLng(newPos);
    //map.setView(newPos, map.getZoom());

    updateSunPosition();
    updateMoonPosition();
}

const orbitPath = L.polyline([], {
    color: '#FD71CB',
    weight: 3,
    dashArray: '10, 10',
    opacity: 0.8,
}).addTo(map);

async function drawFutureOrbit() {
    try {
        const response = await fetch(
            'https://api.wheretheiss.at/v1/satellites/25544/tles',
        );
        const data = await response.json();

        if (typeof satellite === 'undefined') {
            console.error(
                'ERROR: satellite.js not found!',
            );
            return;
        }

        const satrec = satellite.twoline2satrec(data.line1, data.line2);
        const latlngs = [];
        const now = new Date();

        for (let i = 0; i <= 90; i++) {
            const futureTime = new Date(now.getTime() + i * 60000);
            const positionAndVelocity = satellite.propagate(satrec, futureTime);
            const gmst = satellite.gstime(futureTime);
            const positionGd = satellite.eciToGeodetic(
                positionAndVelocity.position,
                gmst,
            );

            const lat = satellite.degreesLat(positionGd.latitude);
            const lng = satellite.degreesLong(positionGd.longitude);
            latlngs.push([lat, lng]);
        }

        const correctedPaths = [];
        let currentSegment = [];

        for (let i = 0; i < latlngs.length; i++) {
            currentSegment.push(latlngs[i]);
            if (
                i < latlngs.length - 1 &&
                Math.abs(latlngs[i][1] - latlngs[i + 1][1]) > 180
            ) {
                correctedPaths.push(currentSegment);
                currentSegment = [];
            }
        }
        correctedPaths.push(currentSegment);

        orbitPath.setLatLngs(correctedPaths);
    } catch (error) {
        console.error('Error:', error);
    }
}

updateSunPosition();
updateMoonPosition();
drawFutureOrbit();

setInterval(drawFutureOrbit, 900000);

setInterval(updateSunPosition, 60000);
setInterval(updateMoonPosition, 60000);
