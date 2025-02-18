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
    var countdownElement = document.getElementById("countdown");

    if (countdownElement) {
        var countdown = parseInt(countdownElement.innerText.trim(), 10);

        if (!isNaN(countdown) && countdown > 0) {
            countdownElement.innerText = countdown;

            while (countdown > 0) {
                await new Promise(resolve => setTimeout(resolve, 1000)); // Espera 1 segundo
                countdown--; // Reducir el contador
                countdownElement.innerText = countdown; // Actualizar la vista
                location.reload();
                
            }

            // Cuando llegue a 0, recargar la página
            location.reload();
        }
    }

});