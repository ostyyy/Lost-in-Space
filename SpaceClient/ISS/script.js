
    var map = L.map('map').setView([0, 0], 3);

    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; OpenStreetMap contributors &copy; CARTO',
    maxZoom: 10
        }).addTo(map);

    var issIcon = L.divIcon({className: 'iss-icon', iconSize: [40, 40], iconAnchor: [20, 20] });
    var issMarker = L.marker([0, 0], {icon: issIcon}).addTo(map);

    async function getISSLocation() {
            try {
                const response = await fetch('https://api.wheretheiss.at/v1/satellites/25544');
    const data = await response.json();

    const lat = data.latitude;
    const lon = data.longitude;

    issMarker.setLatLng([lat, lon]);

    map.setView([lat, lon], map.getZoom(), {animate: true });

            } catch (error) {
        console.error("Error!", error);
            }
        }

    getISSLocation();
    setInterval(getISSLocation, 1000);