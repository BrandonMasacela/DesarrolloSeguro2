// Definir la variable token al inicio del script
let token;
document.addEventListener("DOMContentLoaded", function (event) {

    // Obtener el token del almacenamiento local
    token = localStorage.getItem('token');

    // Verificar si el token existe
    if (!token) {
        $.LoadingOverlay("hide");
        mostrarMensajeError("No se encontró el token de autenticación.");
        return;
    }
    let tablaSolicitudes = $('#tbSolicitudes').DataTable({
        responsive: true,
        scrollX: true,
        scrollY: '50vh',
        scrollCollapse: true,
        autoWidth: false,
        "ajax": {
            "url": `/SolicitudPrestamo/ObtenerSolicitudesPendientes`,
            "type": "GET",
            'Authorization': `Bearer ${token}`,
            "datatype": "json"
        },
        "columns": [
            { 
                title: "ID", 
                data: "id",
                className: 'col-id fixed-column',
                width: "80px"
            },
            { 
                title: "Usuario", 
                data: "cedula",
                className: 'col-usuario',
                width: "120px"
            },
            { 
                title: "Monto", 
                data: "monto",
                className: 'col-monto',
                width: "120px"
            },
            { 
                title: "Plazo", 
                data: "plazo",
                className: 'col-plazo',
                width: "100px"
            },
            { 
                title: "Método de Pago", 
                data: "metodoPago",
                className: 'col-metodo',
                width: "150px"
            },
            { 
                title: "Estado", 
                data: "estado",
                className: 'col-estado',
                width: "100px"
            },
            {
                title: "Acciones", 
                data: "id",
                className: 'col-acciones',
                width: "200px",
                orderable: false,
                render: function (data, type, row) {
                    return `
                        <button class="btn btn-success btn-action btn-aceptar" data-id="${data}" title="Aceptar">
                            <i class="fas fa-check"></i>
                        </button>
                        <button class="btn btn-danger btn-action btn-rechazar" data-id="${data}" title="Rechazar">
                            <i class="fas fa-times"></i>
                        </button>
                    `;
                }
            }
        ],
        "order": [[0, 'desc']],
        fixedColumns: {
            left: 1
        },
        language: {
            search: "Buscar:",
            lengthMenu: "Mostrar _MENU_ registros por página",
            info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
            infoEmpty: "Mostrando 0 a 0 de 0 registros",
            infoFiltered: "(filtrado de _MAX_ registros totales)",
            zeroRecords: "No se encontraron resultados",
            emptyTable: "Ningún dato disponible en esta tabla",
            paginate: {
                first: "Primero",
                last: "Último",
                next: "Siguiente",
                previous: "Anterior"
            }
        },
        dom: '<"row"<"col-sm-12 col-md-6"f><"col-sm-12 col-md-6"l>>' +
             '<"row"<"col-sm-12"tr>>' +
             '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        drawCallback: function() {
            $('.dataTables_scrollBody').css('min-height', '300px');
        }
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
                    mostrarMensajeExito("Estado actualizado con éxito");
                    tablaSolicitudes.ajax.reload();
                }
            } else {
                mostrarMensajeError(result.message || "Error al actualizar el estado");
            }
        } else {
            mostrarMensajeError("Error al obtener la solicitud");
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
                mostrarMensajeExito("Estado actualizado con éxito");
                tablaSolicitudes.ajax.reload();
            } else {
                mostrarMensajeError(result.message || "Error al actualizar el estado");
            }
        } else {
            mostrarMensajeError("Error al obtener la solicitud");
        }
    }
});