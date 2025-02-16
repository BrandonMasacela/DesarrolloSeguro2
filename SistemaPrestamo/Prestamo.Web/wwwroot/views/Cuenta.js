document.addEventListener("DOMContentLoaded", function (event) {
    const idClienteElement = document.getElementById("idCliente");
    const idCliente = idClienteElement ? idClienteElement.value : null;

    if (!idCliente) {
        console.error("No se pudo obtener el ID del cliente");
        return;
    }

    console.log("Id Cliente: " + idCliente);

    // Obtener los datos de la cuenta
    fetch(`/Cuenta/ObtenerCuenta?idCliente=${idCliente}`, {
        method: "GET",
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error en la respuesta del servidor');
            }
            return response.json();
        })
        .then(responseJson => {
            console.log("Respuesta del servidor:", responseJson); // Para depuración

            if (responseJson.success && responseJson.data) {
                const cuenta = responseJson.data;
                document.getElementById("txtTarjeta").textContent = cuenta.tarjeta;
                document.getElementById("txtMonto").textContent = cuenta.monto.toFixed(2);
            } else {
                throw new Error(responseJson.message || 'No se pudo obtener los datos de la cuenta');
            }
        })
        .catch((error) => {
            console.error("Error:", error); // Para depuración
            Swal.fire({
                title: "Error!",
                text: "No se pudo obtener los datos de la cuenta.",
                icon: "warning"
            });
        });


    // Abrir modal de depósito
    document.getElementById("btnDepositar").addEventListener("click", function () {
        $('#modalDeposito').modal('show');
    });

    // Confirmar depósito
    document.getElementById("btnConfirmarDeposito").addEventListener("click", function () {
        const monto = parseFloat(document.getElementById("txtMontoDeposito").value);
        if (isNaN(monto) || monto <= 0) {
            Swal.fire({
                title: "Error!",
                text: "Ingrese un monto válido.",
                icon: "warning"
            });
            return;
        }

        const requestData = {
            idCliente: idCliente,
            monto: monto
        };

        fetch('/Cliente/Depositar', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData)
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    Swal.fire({
                        title: "Éxito!",
                        text: "Depósito realizado correctamente.",
                        icon: "success"
                    }).then(() => {
                        window.location.reload();
                    });
                } else {
                    Swal.fire({
                        title: "Error!",
                        text: data.error || "Error al realizar el depósito.",
                        icon: "error"
                    });
                }
            })
            .catch(error => {
                Swal.fire({
                    title: "Error!",
                    text: "Error al realizar el depósito.",
                    icon: "error"
                });
                console.error("Error al realizar el depósito:", error);
            });
    });

    // Cerrar modal al hacer clic en el botón de cancelar
    document.querySelector("#modalDeposito .btn-secondary").addEventListener("click", function () {
        $('#modalDeposito').modal('hide');
    });

    // Cerrar modal al hacer clic en la "x"
    document.querySelector("#modalDeposito .close").addEventListener("click", function () {
        $('#modalDeposito').modal('hide');
    });
});