let token;
$(document).ready(function () {
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

    $("#btnSolicitarCodigo").click(function () {
        const contrasenaActual = $("#txtContrasenaActual").val();
        const nuevaContrasena = $("#txtNuevaContrasena").val();
        const confirmarContrasena = $("#txtConfirmarContrasena").val();

        // Obtener el token del almacenamiento local
        const token = localStorage.getItem('token');

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


        if (!contrasenaActual || !nuevaContrasena || !confirmarContrasena) {
            Swal.fire('Error', 'Por favor complete todos los campos', 'error');
            return;
        }

        if (nuevaContrasena !== confirmarContrasena) {
            Swal.fire('Error', 'Las contraseñas no coinciden', 'error');
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
                    Swal.fire('Error', data.message, 'error');
                }
            });
    });

    $("#verifyCodeButton").click(function () {
        const codigo = $("#verificationCode").val();

        if (!codigo) {
            Swal.fire('Error', 'Por favor ingrese el código de verificación', 'error');
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
                    Swal.fire('Éxito', 'Contraseña cambiada correctamente', 'success')
                        .then(() => {
                            window.location.href = '/Home/Index';
                        });
                } else {
                    Swal.fire('Error', data.message, 'error');
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