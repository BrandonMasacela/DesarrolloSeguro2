/*!
    * Start Bootstrap - SB Admin v7.0.7 (https://startbootstrap.com/template/sb-admin)
    * Copyright 2013-2023 Start Bootstrap
    * Licensed under MIT (https://github.com/StartBootstrap/startbootstrap-sb-admin/blob/master/LICENSE)
    */
// 
// Scripts
// 

window.addEventListener('DOMContentLoaded', event => {

    // Toggle the side navigation
    const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        // Uncomment Below to persist sidebar toggle between refreshes
        // if (localStorage.getItem('sb|sidebar-toggle') === 'true') {
        //     document.body.classList.toggle('sb-sidenav-toggled');
        // }
        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sb-sidenav-toggled');
            localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sb-sidenav-toggled'));
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