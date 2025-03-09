$(document).ready(function () {
    // Constantes para el manejo de intentos
    const MAX_INTENTOS = 3;
    const TIEMPO_BLOQUEO = 300; // 5 minutos en segundos

    // Verificar si hay un bloqueo activo
    verificarBloqueo();

    $("#btnIniciarSesion").click(async function () {
        if (estaEnBloqueo()) {
            mostrarMensajeError("Debe esperar que termine el tiempo de bloqueo");
            return;
        }

        const correo = $("#txtCorreo").val();
        const clave = $("#txtClave").val();

        if (!correo || !clave) {
            mostrarMensajeError("Por favor complete todos los campos");
            return;
        }

        $.LoadingOverlay("show");

        try {
            const response = await fetch('/Account/Login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    Correo: correo,
                    Clave: clave
                })
            });

            const data = await response.json();
            $.LoadingOverlay("hide");

            if (data.success) {
                // Limpiar intentos fallidos si el login es exitoso
                limpiarIntentosFallidos();
                
                // Guardar el token
                localStorage.setItem('token', data.token);
                
                // Redirigir según el rol
                window.location.href = data.redirectUrl;
            } else {
                manejarIntentoFallido();
                mostrarMensajeError(data.message);
            }
        } catch (error) {
            $.LoadingOverlay("hide");
            mostrarMensajeError("Ocurrió un error al iniciar sesión");
        }
    });

    function manejarIntentoFallido() {
        let intentos = obtenerIntentosFallidos();
        intentos++;
        localStorage.setItem('intentosFallidos', intentos);

        if (intentos >= MAX_INTENTOS) {
            const tiempoBloqueo = Date.now() + (TIEMPO_BLOQUEO * 1000);
            localStorage.setItem('tiempoBloqueo', tiempoBloqueo);
            iniciarContadorBloqueo();
        }
    }

    function obtenerIntentosFallidos() {
        return parseInt(localStorage.getItem('intentosFallidos') || '0');
    }

    function limpiarIntentosFallidos() {
        localStorage.removeItem('intentosFallidos');
        localStorage.removeItem('tiempoBloqueo');
    }

    function estaEnBloqueo() {
        const tiempoBloqueo = localStorage.getItem('tiempoBloqueo');
        if (!tiempoBloqueo) return false;

        const tiempoRestante = parseInt(tiempoBloqueo) - Date.now();
        return tiempoRestante > 0;
    }

    function verificarBloqueo() {
        if (estaEnBloqueo()) {
            iniciarContadorBloqueo();
            $("#btnIniciarSesion").prop('disabled', true);
        } else {
            const intentos = obtenerIntentosFallidos();
            if (intentos > 0) {
                mostrarIntentosRestantes(MAX_INTENTOS - intentos);
            }
        }
    }

    function iniciarContadorBloqueo() {
        const tiempoBloqueo = localStorage.getItem('tiempoBloqueo');
        if (!tiempoBloqueo) return;

        const actualizarContador = () => {
            const tiempoRestante = Math.max(0, parseInt(tiempoBloqueo) - Date.now());
            
            if (tiempoRestante <= 0) {
                limpiarIntentosFallidos();
                $("#mensajeBloqueo").hide();
                $("#btnIniciarSesion").prop('disabled', false);
                return;
            }

            const segundos = Math.ceil(tiempoRestante / 1000);
            const minutos = Math.floor(segundos / 60);
            const segundosRestantes = segundos % 60;

            $("#mensajeBloqueo")
                .show()
                .html(`Cuenta bloqueada. Espere ${minutos}:${segundosRestantes.toString().padStart(2, '0')} minutos`);

            setTimeout(actualizarContador, 1000);
        };

        actualizarContador();
    }

    function mostrarIntentosRestantes(intentos) {
        $("#mensajeIntentos")
            .show()
            .html(`Intentos restantes: ${intentos}`);
    }
}); 