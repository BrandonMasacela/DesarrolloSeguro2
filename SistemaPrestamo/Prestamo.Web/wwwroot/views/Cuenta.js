document.addEventListener("DOMContentLoaded", function (event) {
    const idCliente = @Model.IdCliente;
    console.log("Id Cliente: "+idCliente);

    // Obtener los datos de la cuenta
    fetch(`/Cliente/Cuenta?idCliente=${idCliente}`, {
        method: "GET",
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        if (responseJson.data) {
            const cuenta = responseJson.data;
            $("#txtTarjeta").val(cuenta.tarjeta);
            $("#txtMonto").val(cuenta.monto);
        }
    }).catch((error) => {
        Swal.fire({
            title: "Error!",
            text: "No se pudo obtener los datos de la cuenta.",
            icon: "warning"
        });
    });

    // Manejar el evento de clic en el botón Depositar
    $("#btnDepositar").on("click", function () {
        $("#modalDeposito").modal("show");
    });

    // Manejar el evento de clic en el botón Confirmar
    $("#btnConfirmarDeposito").on("click", function () {
        const montoDeposito = parseFloat($("#txtMontoDeposito").val());
        if (isNaN(montoDeposito) || montoDeposito <= 0) {
            Swal.fire({
                title: "Error!",
                text: "Debe ingresar un monto válido.",
                icon: "warning"
            });
            return;
        }

        // Realizar el depósito
        fetch(`/Cliente/Depositar`, {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify({ idCliente: idCliente, monto: montoDeposito })
        }).then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.data == "") {
                Swal.fire({
                    title: "Listo!",
                    text: "Depósito realizado con éxito.",
                    icon: "success"
                });
                $("#modalDeposito").modal("hide");
                $("#txtMonto").val((parseFloat($("#txtMonto").val()) + montoDeposito).toFixed(2));
            } else {
                Swal.fire({
                    title: "Error!",
                    text: responseJson.data,
                    icon: "warning"
                });
            }
        }).catch((error) => {
            Swal.fire({
                title: "Error!",
                text: "No se pudo realizar el depósito.",
                icon: "warning"
            });
        });
    });
});