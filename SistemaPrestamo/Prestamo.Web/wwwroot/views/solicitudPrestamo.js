// Definir las variables al inicio
let token;
let nroDocumentoCliente;

// Ejecutar cuando la vista cargue
document.addEventListener('DOMContentLoaded', async function () {
    token = localStorage.getItem('token');

    if (!token) {
        $.LoadingOverlay("hide");
        mostrarMensajeAdvertencia("No se encontró el token de autenticación.");
        return;
    }

    try {
        // Obtener el número de cédula del cliente autenticado
        const responseCedula = await fetch('/SolicitudPrestamo/ObtenerCedulaCliente', {
            method: "GET",
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json;charset=utf-8'
            }
        });

        if (!responseCedula.ok) throw new Error("No se pudo obtener la cédula del cliente.");

        const responseJson = await responseCedula.json();
        nroDocumentoCliente = responseJson.cedula;

        // Asignar la cédula al input y deshabilitarlo
        document.getElementById("txtCedula").value = nroDocumentoCliente;
        document.getElementById("txtCedula").disabled = true;

    } catch (error) {
        console.error("Error al obtener la cédula del cliente:", error);
        mostrarMensajeError("No se pudo obtener la cédula del cliente.");
    }
});

// Evento para enviar el formulario
document.getElementById('solicitudForm').addEventListener('submit', async function (e) {
    e.preventDefault(); // Prevenir el envío por defecto del formulario

    if (!token) {
        mostrarMensajeAdvertencia("No se encontró el token de autenticación.");
        return;
    }

    if (!nroDocumentoCliente) {
        mostrarMensajeError("No se pudo obtener la cédula del cliente.");
        return;
    }

    try {
        // Obtener los datos del formulario
        const formData = new FormData(e.target);
        const data = Object.fromEntries(formData.entries());

        // Agregar la cédula obtenida al objeto data
        data.cedula = nroDocumentoCliente;
        data.EsCasado = document.getElementById('cboCasado').value === 'true';

        // Enviar la solicitud
        const responseSolicitud = await fetch('/SolicitudPrestamo/CrearSolicitud', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        const result = await responseSolicitud.json();

        if (result.success) {
            await mostrarMensajeExito("La solicitud ha sido enviada correctamente.");
            document.getElementById('solicitudForm').reset();
        } else {
            console.error("Error al enviar la solicitud:", result.message);
            await mostrarMensajeError(result.message || "No se pudo enviar la solicitud.");
        }

    } catch (error) {
        console.error("Error en el proceso:", error);
        mostrarMensajeError("Hubo un error al procesar la solicitud.");
    }
});
