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
    $.LoadingOverlay("show");
    setTimeout(() => {

    // Obtener el rol del usuario
    fetch(`/Home/ObtenerRolUsuario`, {
        method: "GET",
        headers: {
            'Content-Type': 'application/json;charset=utf-8',
            'Authorization': `Bearer ${token}`
        }
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        const roles = responseJson.roles;
        if (roles.includes("Administrador")) {
            // Si el usuario es Administrador, obtener el resumen administrativo
            fetch(`/Home/ObtenerResumen`, {

                method: "GET",
                headers: {
                    'Content-Type': 'application/json;charset=utf-8',
                    'Authorization': `Bearer ${token}`
                }
            }).then(response => {
                return response.ok ? response.json() : Promise.reject(response);
            }).then(responseJson => {
                $.LoadingOverlay("hide");
                console.log(responseJson.data);
                if (responseJson.data != undefined) {
                    const r = responseJson.data;
                    $("#spInteresAcumulado").text(r.interesAcumulado);
                    $("#spPrestamosCancelados").text(r.prestamosCancelados);
                    $("#spPrestamosPendientes").text(r.prestamosPendientes);
                    $("#spSolicitudes").text(r.solicitudesPendientes);
                    $("#spTotalClientes").text(r.totalClientes);
                }
            }).catch((error) => {
                $.LoadingOverlay("hide");
                Swal.fire({
                    title: "Error!",
                    text: "No se pudo obtener el resumen administrativo.",
                    icon: "warning"
                });
            });
        } else if (roles.includes("Cliente")) {
            // Si el usuario es Cliente, obtener el resumen del cliente
            fetch(`/Home/ObtenerResumenCliente`, {
                method: "GET",
                headers: {
                    'Content-Type': 'application/json;charset=utf-8',
                    'Authorization': `Bearer ${token}`
                }
            }).then(response => {
                return response.ok ? response.json() : Promise.reject(response);
            }).then(responseJson => {
                $.LoadingOverlay("hide");
                console.log(responseJson.data);
                if (responseJson != undefined) {
                    const r = responseJson;
                    $("#spTusPrestamos").text(r.pagosClientePendientes);
                    $("#spPagosPendientes").text(r.prestamosCliente);
                }
            }).catch((error) => {
                $.LoadingOverlay("hide");
                Swal.fire({
                    title: "Error!",
                    text: "No se pudo obtener el resumen del cliente.",
                    icon: "warning"
                });
            });
        } else {
            $.LoadingOverlay("hide");
            Swal.fire({
                title: "Error!",
                text: "Rol de usuario no reconocido.",
                icon: "warning"
            });
        }
    }).catch((error) => {
        $.LoadingOverlay("hide");
        Swal.fire({
            title: "Error!",
            text: "No se pudo obtener el rol del usuario.",
            icon: "warning"
        });
    });
    }, 2000);
});