let tablaData;
let idEditar = 0;
const controlador = "Prestamo";
const confirmaRegistro = "Prestamo registrado!";

let idCliente = 0;
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

    $.LoadingOverlay("show");
    fetch(`/Moneda/Lista`, {
        method: "GET",
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json;charset=utf-8'
        }
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        $.LoadingOverlay("hide");
        if (responseJson.data.length > 0) {
            responseJson.data.forEach((item) => {
                $("#cboTipoMoneda").append($("<option>").val(item.idMoneda).text(item.nombre));
            });
        }
    }).catch((error) => {
        $.LoadingOverlay("hide");
        mostrarMensajeError("No se pudo obtener la lista de monedas.");
    })
});


$("#btnBuscar").on("click", function () {

    if ($("#txtNroDocumento").val() == "") {
        mostrarMensajeAdvertencia("Debe ingresar un número de documento.");
        return;
    }

    $("#cardCliente").LoadingOverlay("show");

    fetch(`/${controlador}/ObtenerCliente?NroDocumento=${$("#txtNroDocumento").val()}`, {
        method: "GET",
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json;charset=utf-8'
        }
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        $("#cardCliente").LoadingOverlay("hide");
        if (responseJson.data.idCliente != 0) {

            const cliente = responseJson.data;
            idCliente = cliente.idCliente;
            $("#txtNombre").val(cliente.nombre);
            $("#txtApellido").val(cliente.apellido);
            $("#txtCorreo").val(cliente.correo);
            $("#txtTelefono").val(cliente.telefono);

        } else {
            $("#txtNombre").val('');
            $("#txtApellido").val('');
            $("#txtCorreo").val('');
            $("#txtTelefono").val('');
            mostrarMensajeAdvertencia("No se encontró un cliente registrado. ¿Desea registrar manualmente?", "Advertencia", true)
                .then((result) => {
                    if (result) {
                        idCliente = 0;
                        $("#txtNombre").removeAttr('disabled');
                        $("#txtApellido").removeAttr('disabled');
                        $("#txtCorreo").removeAttr('disabled');
                        $("#txtTelefono").removeAttr('disabled');
                    }
                });
        }
    }).catch((error) => {
        $("#cardCliente").LoadingOverlay("hide");
        mostrarMensajeError("No se pudo obtener el cliente.");
    })

})

$("#btnCalcular").on("click", function () {
    const inputsPrestamo = $(".data-prestamo").serializeArray();
    const inputText = inputsPrestamo.find((e) => e.value == "");

    if (inputText != undefined) {
        mostrarMensajeAdvertencia(`Debe completar el campo: ${inputText.name.replaceAll("_", " ")}`);
        return;
    }
    const montoPrestamo = parseFloat(inputsPrestamo.find((e) => e.name == "Monto_Prestamo").value);
    const interes = parseFloat(inputsPrestamo.find((e) => e.name == "Interes").value);
    const nroCuotas = parseFloat(inputsPrestamo.find((e) => e.name == "NroCuotas").value);

    const montoInteres = montoPrestamo * (interes / 100);
    const montoTotal = montoPrestamo + montoInteres;
    const montoPorCuota = montoTotal / nroCuotas;

    $("#txtMontoInteres").val(montoInteres.toFixed(2));
    $("#txtMontoPorCuota").val(montoPorCuota.toFixed(2));
    $("#txtMontoTotal").val(montoTotal.toFixed(2));

})

$("#btnRegistrar").on("click", function () {
    const inputs = $(".data-in").serializeArray();
    const inputText = inputs.find((e) => e.value == "");

    if (idCliente == 0) {
        if (inputText != undefined) {
            mostrarMensajeAdvertencia(`Debe completar el campo: ${inputText.name.replaceAll("_", " ")}`);
            return;
        }
    }

    if ($("#txtMontoTotal").val() == "") {
        mostrarMensajeAdvertencia("Debe completar el detalle del préstamo.");
        return;
    }

    const objeto = {
        Cliente: {
            IdCliente: idCliente,
            NroDocumento: $("#txtNroDocumento").val(),
            Nombre: $("#txtNombre").val(),
            Apellido: $("#txtApellido").val(),
            Correo: $("#txtCorreo").val(),
            Telefono: $("#txtTelefono").val()
        },
        Moneda: {
            IdMoneda: $("#cboTipoMoneda").val()
        },
        FechaInicioPago: moment($("#txtFechaInicio").val()).format("DD/MM/YYYY"),
        MontoPrestamo: $("#txtMontoPrestamo").val(),
        InteresPorcentaje: $("#txtInteres").val(),
        NroCuotas: $("#txtNroCuotas").val(),
        FormaDePago: $("#cboFormaPago").val(),
        ValorPorCuota: $("#txtMontoPorCuota").val(),
        ValorInteres: $("#txtMontoInteres").val(),
        ValorTotal: $("#txtMontoTotal").val()
    }

    fetch(`/${controlador}/Crear`, {
        method: "POST",
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(objeto)
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        if (responseJson.data == "") {

            idCliente = 0;
            $(".data-in").val("");
            $(".data-prestamo").val("");
            $("#cboTipoMoneda").val($("#cboTipoMoneda option:first").val());
            $("#cboFormaPago").val($("#cboFormaPago option:first").val());

            mostrarMensajeExito(confirmaRegistro);
        } else {
            mostrarMensajeError(responseJson.data);
        }
    }).catch((error) => {
        mostrarMensajeError("No se pudo registrar el préstamo.");
    })
})