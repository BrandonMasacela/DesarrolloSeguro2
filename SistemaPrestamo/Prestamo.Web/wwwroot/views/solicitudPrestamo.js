// Definir la variable token al inicio del script
let token;
document.getElementById('solicitudForm').addEventListener('submit', async function (e) {
    // Obtener el token del almacenamiento local
    token = localStorage.getItem('token');

    // Verificar si el token existe
    if (!token) {
        $.LoadingOverlay("hide");
        Swal.fire({
            title: "Error!",
            text: "No se encontró el token de autenticación.",
            icon: "warning"
        });
        return;
    }

    e.preventDefault();
    const formData = new FormData(e.target);
    const data = Object.fromEntries(formData.entries());
    data.EsCasado = document.getElementById('cboCasado').value === 'true';

    const response = await fetch('/SolicitudPrestamo/CrearSolicitud', {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
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