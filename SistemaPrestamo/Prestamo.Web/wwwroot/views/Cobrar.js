let idPrestamo = 0;
let totalPagar = 0;
let prestamosEncontrados = [];
let nroDocumentoCliente = "";
// Definir la variable token al inicio del script
let token;
document.addEventListener("DOMContentLoaded", function () {
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
    document.getElementById("txtNumeroTarjeta").addEventListener("input", function (e) {
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
            Swal.fire({
                title: "Ups!",
                text: "No se encontró un cliente.",
                icon: "warning"
            });
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
        Swal.fire({
            title: "Error!",
            text: "No se encontraron resultados.",
            icon: "warning"
        });
    })
}

function obtenerTarjeta() {
    const idCliente = nroDocumentoCliente;
    if (idCliente) {
        fetch(`/Cobrar/ObtenerTarjeta?idCliente=${idCliente}`, {
            method: "GET",
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json;charset=utf-8'
            }
        }).then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            $("#txtNumeroTarjeta").val(responseJson.tarjeta);
        }).catch((error) => {
            console.error("Error al obtener la tarjeta:", error);
        });
    }
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
        Swal.fire({
            title: "Error!",
            text: `No hay prestamo encontrado`,
            icon: "warning"
        });
        return;
    }

    if (totalPagar == 0) {
        Swal.fire({
            title: "Error!",
            text: `No hay cuotas seleccionadas`,
            icon: "warning"
        });
        return;
    }

    Swal.fire({
        title: 'Ingrese el número de tarjeta',
        input: 'text',
        inputAttributes: {
            autocapitalize: 'off'
        },
        cancelButtonText: 'Cancelar',
        showCancelButton: true,
        confirmButtonText: 'Pagar',
        showLoaderOnConfirm: true,
        preConfirm: (tarjeta) => {
            if (!tarjeta) {
                Swal.showValidationMessage('Por favor ingrese el número de tarjeta');
                return false;
            }
            if (!/^\d{16}$/.test(tarjeta)) {
                Swal.showValidationMessage('El número de tarjeta debe contener exactamente 16 dígitos');
                return false;
            }

            const cuotasSeleccionadas = $(".checkPagado:checked").map(function () {
                return $(this).attr("name");
            }).get().join(",");

            if (!cuotasSeleccionadas) {
                Swal.showValidationMessage('Debe seleccionar al menos una cuota');
                return false;
            }

            const requestData = {
                idPrestamo: idPrestamo,
                nroCuotasPagadas: cuotasSeleccionadas,
                numeroTarjeta: tarjeta
            };

            console.log('Enviando datos:', requestData); // Para debugging

            return fetch('/Cobrar/PagarCuotas', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                },
                body: JSON.stringify(requestData)
            })
                .then(response => {
                    if (!response.ok) {
                        return response.json().then(err => Promise.reject(err));
                    }
                    return response.json();
                });
        },
        allowOutsideClick: () => !Swal.isLoading()
    }).then((result) => {
        if (result.isConfirmed) {
            if (result.value.data.startsWith("Error") || result.value.data.includes("incorrecto") || result.value.data.includes("insuficientes")) {
                Swal.fire({
                    title: 'Error!',
                    text: result.value.data,
                    icon: 'error'
                });
            } else {
                Swal.fire({
                    title: 'Éxito!',
                    text: result.value.data,
                    icon: 'success'
                }).then(() => {
                    window.location.reload();
                });
            }
        }
    }).catch(error => {
        Swal.fire({
            title: 'Error!',
            text: error.data || 'Error al procesar el pago',
            icon: 'error'
        });
    });
});