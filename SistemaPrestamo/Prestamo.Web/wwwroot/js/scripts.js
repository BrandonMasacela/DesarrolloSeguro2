window.addEventListener('DOMContentLoaded', event => {
    const loginForm = document.getElementById("loginForm");
    if (loginForm) {
        loginForm.addEventListener("submit", async function (event) {
            event.preventDefault();

            let correo = document.getElementById("correo").value;
            let clave = document.getElementById("clave").value;

            let response = await fetch("/Login/Index", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ correo: correo, clave: clave })
            });

            let data = await response.json();

            if (data.success) {
                localStorage.setItem("token", data.token);
                window.location.href = "/Login/VerificarCodigo";  // Redirigir al dashboard
            } else {
                let errorMessage = document.getElementById("errorMessage");
                errorMessage.innerText = data.message;
                errorMessage.classList.remove("d-none");
            }
        });
    }

    // Lógica para la cuenta regresiva en la página de inicio de sesión
    let tiempoRestante = parseInt(localStorage.getItem("TiempoBloqueado"));
    let countdownElement = document.getElementById("tiempo");
    let mensaje = document.getElementById("alerta");
    let mensaje1 = document.getElementById("alerta1");
    let boton = document.getElementById("btnlogin");
    let correo = document.getElementsByName("correo");
    let contrasena = document.getElementsByName("clave");
    if (tiempoRestante > 0) {

        // Función para actualizar el contador cada segundo
        let timer = setInterval(() => {
            if (tiempoRestante > 0) {
                tiempoRestante--; // Reducir el tiempo
                countdownElement.innerText = tiempoRestante; // Actualizar el HTML
                boton.setAttribute("disabled", "true"); // Deshabilitar el botón
                correo.disabled = true; // Deshabilitar el campo de correo
                contrasena.disabled = true; // Deshabilitar el campo de contraseña
                localStorage.setItem("TiempoBloqueado", tiempoRestante); // Guardar el nuevo valor
            } else {
                clearInterval(timer); // Detener el temporizador cuando llegue a 0
                localStorage.removeItem("TiempoBloqueado"); // Eliminar del localStorage
                mensaje.setAttribute("hidden", "true"); // Ocultar el mensaje
                mensaje1.setAttribute("hidden", "true"); // Ocultar el mensaje
                boton.removeAttribute("disabled"); // Habilitar el botón
                correo.disabled = false;
                contrasena.disabled = false;
            }
        }, 1000); // Ejecutar cada segundo
    }
});