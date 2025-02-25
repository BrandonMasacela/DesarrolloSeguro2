document.getElementById('solicitudForm').addEventListener('submit', async function (e) {
    e.preventDefault();
    const formData = new FormData(e.target);
    const data = Object.fromEntries(formData.entries());
    data.EsCasado = document.getElementById('cboCasado').value === 'true';

    const response = await fetch('/SolicitudPrestamo/CrearSolicitud', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });

    const result = await response.json();
    if (result.success) {
        Swal.fire({
            title: "Listo!",
            text: "Solicitud enviada con éxito",
            icon: "success"
        });
    } else {
        Swal.fire({
            title: "Error!",
            text: "Error al enviar la solicitud: " + result.message,
            icon: "warning"
        });
    }
});