let token;
$(document).ready(function () {
    // Obtener el token del almacenamiento local
    token = localStorage.getItem('token');

    // Verificar si el token existe
    if (!token) {
        $.LoadingOverlay("hide");
        mostrarMensajeError("No se encontró el token de autenticación.");
        return;
    }

    $("#btnSolicitarCodigo").click(async function () {
        const contrasenaActual = $("#txtContrasenaActual").val();
        const nuevaContrasena = $("#txtNuevaContrasena").val();
        const confirmarContrasena = $("#txtConfirmarContrasena").val();

        // Validaciones
        if (!contrasenaActual) {
            await mostrarMensajeError('Por favor ingrese su contraseña actual');
            $("#txtContrasenaActual").focus();
            return;
        }

        if (!nuevaContrasena) {
            await mostrarMensajeError('Por favor ingrese la nueva contraseña');
            $("#txtNuevaContrasena").focus();
            return;
        }

        if (!confirmarContrasena) {
            await mostrarMensajeError('Por favor confirme la nueva contraseña');
            $("#txtConfirmarContrasena").focus();
            return;
        }

        if (nuevaContrasena !== confirmarContrasena) {
            await mostrarMensajeError('Las contraseñas no coinciden');
            $("#txtConfirmarContrasena").focus();
            return;
        }

        // Mostrar indicador de carga
        $.LoadingOverlay("show", {
            text: "Solicitando código de verificación..."
        });

        try {
            // Solicitar código de verificación
            const response = await fetch('/Account/SolicitarCodigoVerificacion', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    CurrentPassword: contrasenaActual,
                    NewPassword: nuevaContrasena,
                    ConfirmPassword: confirmarContrasena
                })
            });

            const data = await response.json();
            $.LoadingOverlay("hide");

            if (data.success) {
                $("#verificationModal").modal('show');
            } else {
                await mostrarMensajeError(data.message || 'Error al solicitar el código de verificación');
            }
        } catch (error) {
            $.LoadingOverlay("hide");
            await mostrarMensajeError('Error al procesar la solicitud');
        }
    });

    $("#verifyCodeButton").click(async function () {
        const codigo = $("#verificationCode").val();
        const nuevaContrasena = $("#txtNuevaContrasena").val();
        const confirmarContrasena = $("#txtConfirmarContrasena").val();

        if (!codigo) {
            await mostrarMensajeError('Por favor ingrese el código de verificación');
            $("#verificationCode").focus();
            return;
        }

        try {
            // Verificar código y cambiar contraseña
            const response = await fetch('/Account/ChangePassword', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    CurrentPassword: $("#txtContrasenaActual").val(),
                    NewPassword: nuevaContrasena,
                    ConfirmPassword: confirmarContrasena,
                    VerificationCode: codigo
                })
            });

            const data = await response.json();
            $.LoadingOverlay("hide");

            if (data.success) {
                await mostrarMensajeExito('Contraseña cambiada correctamente');
                window.location.href = '/Home/Index';
            } else {
                await mostrarMensajeError(data.message || 'Error al cambiar la contraseña');
            }
        } catch (error) {
            $.LoadingOverlay("hide");
            await mostrarMensajeError('Error al procesar la solicitud');
        }
    });

    // Cerrar modal al hacer clic en el botón de cancelar
    $(".modal-footer .btn-secondary").click(function () {
        $('#verificationModal').modal('hide');
    });

    // Cerrar modal al hacer clic en la "x"
    $(".modal-header .close").click(function () {
        $('#verificationModal').modal('hide');
    });
});