// Definir la variable token al inicio del script
let token;
document.addEventListener("DOMContentLoaded", function (event) {

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
    let tablaSolicitudes = $('#tbSolicitudes').DataTable({
        responsive: true,
        scrollX: true,
        "ajax": {
            "url": `/SolicitudPrestamo/ObtenerSolicitudesPendientes`,
            "type": "GET",
            'Authorization': `Bearer ${token}`,
            "datatype": "json"
        },
        "columns": [
            { title: "ID", "data": "id" },
            { title: "Usuario", "data": "cedula" },
            { title: "Monto", "data": "monto" },
            { title: "Plazo", "data": "plazo" },
            { title: "Metodo de Pago", "data": "metodoPago" },
            { title: "Estado", "data": "estado" },
            {
                title: "Acciones", "data": "id", width: "120px", render: function (data, type, row) {
                    return `
                        <button class="btn btn-success me-2 btn-aceptar" data-id="${data}"><i class="fa-solid fa-check"></i> Aceptar</button>
                        <button class="btn btn-danger btn-rechazar" data-id="${data}"><i class="fa-solid fa-times"></i> Rechazar</button>
                    `;
                }
            }
        ],
        "order": [0, 'desc'],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });

    $('#tbSolicitudes tbody').on('click', '.btn-aceptar', function () {
        const id = $(this).data('id');
        aceptarSolicitud(id);
    });

    $('#tbSolicitudes tbody').on('click', '.btn-rechazar', function () {
        const id = $(this).data('id');
        actualizarEstado(id, 'Rechazado');
    });

    async function aceptarSolicitud(id) {
        const response = await fetch(`/SolicitudPrestamo/ObtenerSolicitud/${id}`);
        const solicitud = await response.json();
        if (solicitud.success) {
            // Guardar los datos de la solicitud en el localStorage
            localStorage.setItem('solicitud', JSON.stringify(solicitud.data));

            // Actualizar el estado de la solicitud
            const updateResponse = await fetch('/SolicitudPrestamo/ActualizarEstadoSolicitud', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ id, estado: 'Aprobado' })
            });

            const result = await updateResponse.json();
            if (result.success) {
                if (result.redirectUrl) {
                    // Almacenar el historial crediticio en el localStorage
                    localStorage.setItem('historial', JSON.stringify(result.historial));
                    window.location.href = result.redirectUrl;
                } else {
                    Swal.fire({
                        title: "Listo!",
                        text: "Estado actualizado con éxito",
                        icon: "success"
                    });
                    tablaSolicitudes.ajax.reload();
                }
            } else {
                Swal.fire({
                    title: "Error!",
                    text: result.message || "Error al actualizar el estado",
                    icon: "warning"
                });
            }
        } else {
            Swal.fire({
                title: "Error!",
                text: "Error al obtener la solicitud",
                icon: "warning"
            });
        }
    }

    async function actualizarEstado(id, estado) {
        // Obtener la solicitud actualizada
        const response = await fetch(`/SolicitudPrestamo/ObtenerSolicitud/${id}`);
        const solicitud = await response.json();
        if (solicitud.success) {
            // Guardar los datos de la solicitud en el localStorage
            solicitud.data.estado = estado;
            localStorage.setItem('solicitud', JSON.stringify(solicitud.data));

            // Enviar los datos al servidor
            const updateResponse = await fetch('/SolicitudPrestamo/ActualizarEstadoSolicitud', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ id, estado })
            });

            const result = await updateResponse.json();
            if (result.success) {
                Swal.fire({
                    title: "Listo!",
                    text: "Estado actualizado con éxito",
                    icon: "success"
                });
                tablaSolicitudes.ajax.reload();
            } else {
                Swal.fire({
                    title: "Error!",
                    text: result.message || "Error al actualizar el estado",
                    icon: "warning"
                });
            }
        } else {
            Swal.fire({
                title: "Error!",
                text: "Error al obtener la solicitud",
                icon: "warning"
            });
        }
    }
});