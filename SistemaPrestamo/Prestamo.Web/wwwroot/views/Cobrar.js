let idPrestamo = 0;
let totalPagar = 0;
let prestamosEncontrados = [];
let nroDocumentoCliente = "";
let idCliente;

// Definir la variable token al inicio del script
let token;
document.addEventListener("DOMContentLoaded", function () {
    // Obtener el token del almacenamiento local
    token = localStorage.getItem('token');

    // Verificar si el token existe
    if (!token) {
        $.LoadingOverlay("hide");
        mostrarMensajeError("No se encontró el token de autenticación.");
        return;
    }

    const idClienteElement = document.getElementById("idCliente");
    idCliente = idClienteElement ? idClienteElement.value : null;

    if (!idCliente) {
        console.error("No se pudo obtener el ID del cliente");
        return;
    }


    // Obtener el número de cédula del cliente autenticado
    fetch('/Prestamo/ObtenerCedulaCliente', {
        method: "GET",
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json;charset=utf-8'
        }
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        nroDocumentoCliente = responseJson.cedula;
        buscarPrestamos();
    }).catch(error => {
        console.error("Error al obtener la cédula del cliente:", error);
    });

    // Validar que solo se ingresen números en el campo de número de tarjeta
    document.getElementById("txtNumeroTarjetaModal").addEventListener("input", function (e) {
        this.value = this.value.replace(/\D/g, '');
    });
});

function buscarPrestamos() {
    $.LoadingOverlay("show");
    fetch(`/Prestamo/ObtenerPrestamos?IdPrestamo=0&NroDocumento=${nroDocumentoCliente}`, {
        method: "GET",
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json;charset=utf-8'
        }
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        $.LoadingOverlay("hide");
        prestamosEncontrados = [];

        if (responseJson.data.length == 0) {
            Limpiar(false);
            mostrarMensajeAdvertencia("No se encontró un cliente.");
            return;
        }

        if (responseJson.data.length == 1) {
            let dataFiltro = []
            dataFiltro = responseJson.data.filter((e) => e.estado == "Pendiente");

            if (dataFiltro.length == 0) {
                dataFiltro = responseJson.data.filter((e) => e.estado == "Cancelado");
            }

            const prestamo = dataFiltro[0];
            mostrarPrestamo(prestamo);
        } else {
            Limpiar(false);
            prestamosEncontrados = responseJson.data;

            $("#tbPrestamosEncontrados tbody").html("");
            responseJson.data.forEach(function (e) {
                $("#tbPrestamosEncontrados tbody").append(`<tr>
                        <td><button class="btn btn-primary btn-sm btn-prestamo-encontrado" data-idprestamo="${e.idPrestamo}"><i class="fa-solid fa-check"></i></button></td>
                        <td>${e.idPrestamo}</td>
                        <td>${e.montoPrestamo}</td>
                        <td>${e.estado == "Pendiente" ? '<span class="badge bg-danger p-2">Pendiente</span>' : '<span class="badge bg-success p-2">Cancelado</span>'}</td>
                        <td>${e.fechaCreacion}</td>
                    </tr>`);
            })
            $(`#mdData`).modal('show');
        }
    }).catch((error) => {
        $.LoadingOverlay("hide");
        mostrarMensajeError("No se encontraron resultados.");
    })
}

function Limpiar(limpiarNroDocumento) {
    if (limpiarNroDocumento)
        $("#txtNroDocumento").val("");

    idPrestamo = 0;
    totalPagar = 0;
    $("#txtNroPrestamo").val("");
    $("#txtNombreCliente").val("");
    $("#txtMontoPrestamo").val("");
    $("#txtInteres").val("");
    $("#txtNroCuotas").val("");
    $("#txtMontoTotal").val("");
    $("#txtFormadePago").val("");
    $("#txtTipoMoneda").val("");
    $("#txtTotalaPagar").val("");
    $("#tbDetalle tbody").html("");
}

function mostrarPrestamo(prestamo) {
    idPrestamo = prestamo.idPrestamo;

    $("#txtNroPrestamo").val(prestamo.idPrestamo);
    $("#txtNombreCliente").val(`${prestamo.cliente.nombre} ${prestamo.cliente.apellido}`);
    $("#txtMontoPrestamo").val(prestamo.montoPrestamo);
    $("#txtInteres").val(prestamo.interesPorcentaje);
    $("#txtNroCuotas").val(prestamo.nroCuotas);
    $("#txtMontoTotal").val(prestamo.valorTotal);
    $("#txtFormadePago").val(prestamo.formaDePago);
    $("#txtTipoMoneda").val(prestamo.moneda.nombre);

    $("#tbDetalle tbody").html("");
    prestamo.prestamoDetalle.forEach(function (e) {
        const activar = e.estado == 'Cancelado' ? 'disabled checked' : '';
        const clase = e.estado == 'Cancelado' ? '' : 'checkPagado';

        $("#tbDetalle tbody").append(`<tr>
                        <td><input class="form-check-input ${clase}" type="checkbox" name="${e.nroCuota}" data-monto=${e.montoCuota} data-idprestamodetalle=${e.idPrestamoDetalle} ${activar}/></td>
                        <td>${e.nroCuota}</td>
                        <td>${e.fechaPago}</td>
                        <td>${e.montoCuota}</td>
                        <td>${e.estado == "Pendiente" ? '<span class="badge bg-danger p-2">Pendiente</span>' : '<span class="badge bg-success p-2">Cancelado</span>'}</td>
                        <td>${e.fechaPagado}</td>
                    </tr>`);
    })
}

$(document).on('click', '.checkPagado', function (e) {
    const seleccionados = $(".checkPagado").serializeArray();
    const nroCuota = $(this).attr("name").toString();

    const encontrado = seleccionados.find((i) => i.name == nroCuota);
    if (encontrado != undefined) {
        totalPagar = totalPagar + parseFloat($(this).data("monto"));
    } else {
        totalPagar = totalPagar - parseFloat($(this).data("monto"));
    }
    $("#txtTotalaPagar").val(totalPagar.toFixed(2));
});

$(document).on('click', '.btn-prestamo-encontrado', function (e) {
    const idPrestamo = parseInt($(this).data("idprestamo"));
    const prestamo = prestamosEncontrados.find((e) => e.idPrestamo == idPrestamo);
    mostrarPrestamo(prestamo);
    $(`#mdData`).modal('hide');
});

$("#btnRegistrarPago").on("click", function () {
    if (idPrestamo == 0) {
        mostrarMensajeError("No hay prestamo encontrado");
        return;
    }

    if (totalPagar == 0) {
        mostrarMensajeError("No hay cuotas seleccionadas");
        return;
    }

    $('#modalIngresarTarjeta').modal('show');
});

$("#btnConfirmarTarjeta").on("click", async function () {
    const numeroTarjeta = document.getElementById("txtNumeroTarjetaModal").value;
    if (!/^\d{16}$/.test(numeroTarjeta)) {
        mostrarMensajeError("El número de tarjeta debe contener exactamente 16 dígitos.");
        return;
    }

    try {
        const response = await fetch(`/Cuenta/ObtenerCuenta?idCliente=${idCliente}`, {
            method: "GET",
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json;charset=utf-8'
            }
        });

        const responseJson = await response.json();
        const tarjetaObtenida = responseJson.data.tarjeta ? responseJson.data.tarjeta.toString() : null;
        console.log('Tarjeta obtenida:', tarjetaObtenida); // Para debugging


        if (numeroTarjeta != tarjetaObtenida) {
            mostrarMensajeError("Tarjeta Invalida!");
            return;
        }

        const cuotasSeleccionadas = $(".checkPagado:checked").map(function () {
            return $(this).attr("name");
        }).get().join(",");

        if (!cuotasSeleccionadas) {
            mostrarMensajeError("Debe seleccionar al menos una cuota");
            return;
        }

        const requestData = {
            idPrestamo: idPrestamo,
            nroCuotasPagadas: cuotasSeleccionadas,
            numeroTarjeta: numeroTarjeta
        };

        console.log('Enviando datos:', requestData); // Para debugging

        const result = await fetch('/Cobrar/PagarCuotas', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
                'Accept': 'application/json'
            },
            body: JSON.stringify(requestData)
        }).then(response => {
            if (!response.ok) {
                return response.json().then(err => Promise.reject(err));
            }
            return response.json();
        });

        if (result.data.startsWith("Error") || result.data.includes("incorrecto") || result.data.includes("insuficientes")) {
            mostrarMensajeError(result.data);
        } else {
            mostrarMensajeExito(result.data).then(() => {
                window.location.reload();
            });
        }
    } catch (error) {
        mostrarMensajeError(error.data || 'Error al procesar el pago');
    }
});

// Cerrar modal al hacer clic en el botón de cancelar
document.querySelector("#modalIngresarTarjeta .btn-secondary").addEventListener("click", function () {
    $('#modalIngresarTarjeta').modal('hide');
});

// Cerrar modal al hacer clic en la "x"
document.querySelector("#modalIngresarTarjeta .close").addEventListener("click", function () {
    $('#modalIngresarTarjeta').modal('hide');
});