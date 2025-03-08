// Definir la variable token al inicio del script
let token;

document.addEventListener('DOMContentLoaded', function () {
    token = localStorage.getItem('token');

    if (!token) {
        $.LoadingOverlay("hide");
        mostrarMensajeAdvertencia("No se encontró el token de autenticación.");
        return;
    }
    cargarDashboard();
});

async function cargarDashboard() {

    mostrarCargando("Cargando información...");

    try {
        // Obtener roles del usuario
        const rolesResponse = await fetch('/Home/ObtenerRolUsuario', {
            method: "GET",
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json;charset=utf-8',
            }
        });

        const rolesJson = await rolesResponse.json();
        const roles = rolesJson.roles;

        // Cargar datos según el rol
        if (roles.includes("Administrador")) {
            await cargarDatosAdmin();
        } else {
            await cargarDatosCliente();
        }

        cerrarAlerta();
    } catch (error) {
        cerrarAlerta();
    }
}

async function cargarDatosAdmin() {
    try {
        const response = await fetch('/Home/ObtenerResumen', {
            method: "GET",
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json;charset=utf-8',
            }
        });

        const responseJson = await response.json();

        if (responseJson.data != undefined) {
            const r = responseJson.data;

            $("#spInteresAcumulado").text(r.interesAcumulado);
            $("#spPrestamosCancelados").text(r.prestamosCancelados);
            $("#spPrestamosPendientes").text(r.prestamosPendientes);
            $("#spSolicitudes").text(r.solicitudesPendientes);
            $("#spTotalClientes").text(r.totalClientes);

            let html = "";
            if (r.ultimosPrestamos.length > 0) {
                r.ultimosPrestamos.forEach((item) => {
                    html += `
                        <div class="row mb-3">
                            <div class="col-sm-12">
                                <div class="card border-left-info shadow h-100 py-2">
                                    <div class="card-body">
                                        <div class="row no-gutters align-items-center">
                                            <div class="col mr-2">
                                                <div class="text-xs font-weight-bold text-info text-uppercase mb-1">Número préstamo: ${item.idPrestamo}</div>
                                                <div class="row no-gutters align-items-center">
                                                    <div class="col-auto">
                                                        <div class="h5 mb-0 mr-3 font-weight-bold text-gray-800">${item.cliente}</div>
                                                        <p class="mb-0">Monto: ${item.monto} - Estado: ${item.estado}</p>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-auto">
                                                <i class="fas fa-dollar-sign fa-2x text-gray-300"></i>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>`;
                });
            }

            $("#contenedor").html(html);
        }
    } catch (error) {
    }
}

async function cargarDatosCliente() {
    try {
        const response = await fetch('/Home/ObtenerResumenCliente', {
            method: "GET",
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json;charset=utf-8',
            }
        });

        const responseJson = await response.json();

        if (responseJson != undefined) {
            const r = responseJson;

            $("#spTusPrestamos").text(r.pagosClientePendientes);
            $("#spPagosPendientes").text(r.prestamosCliente);

            let html = "";
            if (r.ultimosPrestamos.length > 0) {
                r.ultimosPrestamos.forEach((item) => {
                    html += `
                        <div class="row mb-3">
                            <div class="col-sm-12">
                                <div class="card border-left-info shadow h-100 py-2">
                                    <div class="card-body">
                                        <div class="row no-gutters align-items-center">
                                            <div class="col mr-2">
                                                <div class="text-xs font-weight-bold text-info text-uppercase mb-1">Número préstamo: ${item.idPrestamo}</div>
                                                <div class="row no-gutters align-items-center">
                                                    <div class="col-auto">
                                                        <div class="h5 mb-0 mr-3 font-weight-bold text-gray-800">Monto: ${item.monto}</div>
                                                        <p class="mb-0">Estado: ${item.estado}</p>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-auto">
                                                <i class="fas fa-clipboard-list fa-2x text-gray-300"></i>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>`;
                });
            }

            $("#contenedor").html(html);
        }
    } catch (error) {
    }
}