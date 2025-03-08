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

    $("#btnSolicitarCodigo").click(function () {
        const contrasenaActual = $("#txtContrasenaActual").val();
        const nuevaContrasena = $("#txtNuevaContrasena").val();
        const confirmarContrasena = $("#txtConfirmarContrasena").val();

        // Obtener el token del almacenamiento local
        const token = localStorage.getItem('token');

        // Verificar si el token existe
        if (!token) {
            $.LoadingOverlay("hide");
            mostrarMensajeError("No se encontró el token de autenticación.");
            return;
        }

        if (!contrasenaActual || !nuevaContrasena || !confirmarContrasena) {
            mostrarMensajeError('Por favor complete todos los campos');
            return;
        }

        if (nuevaContrasena !== confirmarContrasena) {
            mostrarMensajeError('Las contraseñas no coinciden');
            return;
        }

        // Solicitar código de verificación
        fetch('/Account/SolicitarCodigoVerificacion', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                CurrentPassword: contrasenaActual,
                NewPassword: nuevaContrasena
            })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    $("#verificationModal").modal('show');
                } else {
                    mostrarMensajeError(data.message);
                }
            });
    });

    $("#verifyCodeButton").click(function () {
        const codigo = $("#verificationCode").val();

        if (!codigo) {
            mostrarMensajeError('Por favor ingrese el código de verificación');
            return;
        }

        // Verificar código y cambiar contraseña
        fetch('/Account/ChangePassword', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                VerificationCode: codigo,
                NewPassword: $("#txtNuevaContrasena").val()
            })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    mostrarMensajeExito('Contraseña cambiada correctamente').then(() => {
                        window.location.href = '/Home/Index';
                    });
                } else {
                    mostrarMensajeError(data.message);
                }
            });
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