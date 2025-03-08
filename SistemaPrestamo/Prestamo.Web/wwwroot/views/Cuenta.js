﻿// Definir la variable token al inicio del script
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

    const idClienteElement = document.getElementById("idCliente");
    const idCliente = idClienteElement ? idClienteElement.value : null;

    if (!idCliente) {
        console.error("No se pudo obtener el ID del cliente");
        return;
    }

    // Obtener los datos de la cuenta
    fetch(`/Cuenta/ObtenerCuenta?idCliente=${idCliente}`, {
        method: "GET",
        headers: {
            'Content-Type': 'application/json;charset=utf-8',
            'Authorization': `Bearer ${token}`
        }
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
            mostrarMensajeError("No se pudo obtener los datos de la cuenta.");
        });


    // Abrir modal de depósito
    document.getElementById("btnDepositar").addEventListener("click", function () {
        $('#modalDeposito').modal('show');
    });

    // Confirmar depósito
    document.getElementById("btnConfirmarDeposito").addEventListener("click", function () {
        const monto = parseFloat(document.getElementById("txtMontoDeposito").value);
        if (isNaN(monto) || monto <= 0) {
            mostrarMensajeError("Ingrese un monto válido.");
            return;
        }

        const requestData = {
            idCliente: idCliente,
            monto: monto
        };

        fetch('/Cliente/Depositar', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(requestData)
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    mostrarMensajeExito("Depósito realizado correctamente.").then(() => {
                        window.location.reload();
                    });
                } else {
                    mostrarMensajeError(data.error || "Error al realizar el depósito.");
                }
            })
            .catch(error => {
                mostrarMensajeError("Error al realizar el depósito.");
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