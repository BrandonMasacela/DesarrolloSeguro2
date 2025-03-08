class PrestamoAlerts {
    constructor() {
        this.createContainer();
    }

    createContainer() {
        this.container = document.createElement('div');
        this.container.className = 'prestamo-alert-container';
        document.body.appendChild(this.container);
    }

    fire(options = {}) {
        const {
            title = '',
            text = '',
            icon = '',
            confirmButtonText = 'Aceptar',
            cancelButtonText = 'Cancelar',
            showCancelButton = false,
            timer = 0,
            showLoader = false
        } = options;

        return new Promise((resolve) => {
            const modal = document.createElement('div');
            modal.className = 'prestamo-alert-modal';
            
            let iconHtml = '';
            if (icon || showLoader) {
                iconHtml = `
                    <div class="prestamo-alert-icon prestamo-alert-icon-${showLoader ? 'loader' : icon}">
                        ${showLoader ? this.getLoaderSvg() : this.getIconSvg(icon)}
                    </div>
                `;
            }

            modal.innerHTML = `
                <div class="prestamo-alert-content">
                    ${iconHtml}
                    ${title ? `<h2 class="prestamo-alert-title">${title}</h2>` : ''}
                    ${text ? `<p class="prestamo-alert-text">${text}</p>` : ''}
                    ${!showLoader ? `
                        <div class="prestamo-alert-buttons">
                            ${showCancelButton ? 
                                `<button class="prestamo-alert-button prestamo-alert-cancel">${cancelButtonText}</button>` : 
                                ''}
                            <button class="prestamo-alert-button prestamo-alert-confirm">${confirmButtonText}</button>
                        </div>
                    ` : ''}
                </div>
            `;

            this.container.appendChild(modal);
            setTimeout(() => modal.classList.add('prestamo-alert-show'), 10);

            const close = (result) => {
                modal.classList.remove('prestamo-alert-show');
                setTimeout(() => {
                    this.container.removeChild(modal);
                    resolve(result);
                }, 300);
            };

            if (!showLoader) {
                modal.querySelector('.prestamo-alert-confirm').addEventListener('click', () => close(true));
                if (showCancelButton) {
                    modal.querySelector('.prestamo-alert-cancel').addEventListener('click', () => close(false));
                }
            }

            if (timer > 0) {
                setTimeout(() => close(true), timer);
            }
        });
    }

    getLoaderSvg() {
        return `
            <svg class="prestamo-alert-loader" viewBox="0 0 50 50">
                <circle cx="25" cy="25" r="20" fill="none" stroke="currentColor" stroke-width="5"></circle>
            </svg>
        `;
    }

    getIconSvg(icon) {
        const icons = {
            success: `
                <svg viewBox="0 0 52 52">
                    <circle cx="26" cy="26" r="25" fill="none" stroke="currentColor" stroke-width="2"/>
                    <path fill="none" stroke="currentColor" stroke-width="3" d="M14.1 27.2l7.1 7.2 16.7-16.8"/>
                </svg>
            `,
            error: `
                <svg viewBox="0 0 52 52">
                    <circle cx="26" cy="26" r="25" fill="none" stroke="currentColor" stroke-width="2"/>
                    <line x1="18" y1="18" x2="34" y2="34" stroke="currentColor" stroke-width="3"/>
                    <line x1="34" y1="18" x2="18" y2="34" stroke="currentColor" stroke-width="3"/>
                </svg>
            `,
            warning: `
                <svg viewBox="0 0 52 52">
                    <circle cx="26" cy="26" r="25" fill="none" stroke="currentColor" stroke-width="2"/>
                    <line x1="26" y1="12" x2="26" y2="32" stroke="currentColor" stroke-width="3"/>
                    <circle cx="26" cy="40" r="2" fill="currentColor"/>
                </svg>
            `,
            info: `
                <svg viewBox="0 0 52 52">
                    <circle cx="26" cy="26" r="25" fill="none" stroke="currentColor" stroke-width="2"/>
                    <circle cx="26" cy="15" r="2" fill="currentColor"/>
                    <line x1="26" y1="22" x2="26" y2="40" stroke="currentColor" stroke-width="3"/>
                </svg>
            `
        };
        return icons[icon] || '';
    }
}

// Crear instancia global
window.PrestamoAlert = new PrestamoAlerts();

// Funciones de ayuda globales
window.mostrarMensajeExito = function(mensaje, titulo = "¡Éxito!") {
    return PrestamoAlert.fire({
        title: titulo,
        text: mensaje,
        icon: 'success'
    });
};

window.mostrarMensajeError = function(mensaje, titulo = "Error") {
    return PrestamoAlert.fire({
        title: titulo,
        text: mensaje,
        icon: 'error'
    });
};

window.mostrarMensajeAdvertencia = function(mensaje, titulo = "Advertencia") {
    return PrestamoAlert.fire({
        title: titulo,
        text: mensaje,
        icon: 'warning'
    });
};

window.mostrarMensajeInfo = function(mensaje, titulo = "Información") {
    return PrestamoAlert.fire({
        title: titulo,
        text: mensaje,
        icon: 'info'
    });
};

window.mostrarCargando = function(mensaje = "Procesando...") {
    return PrestamoAlert.fire({
        text: mensaje,
        showLoader: true,
        showConfirmButton: false
    });
};

window.confirmarAccion = function(mensaje, titulo = "¿Está seguro?") {
    return PrestamoAlert.fire({
        title: titulo,
        text: mensaje,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, continuar',
        cancelButtonText: 'No, cancelar'
    });
};

window.cerrarAlerta = function () {
    const container = document.querySelector('.prestamo-alert-container');
    if (container) {
        const modal = container.querySelector('.prestamo-alert-modal');
        if (modal) {
            modal.classList.remove('prestamo-alert-show');
            setTimeout(() => {
                if (container.contains(modal)) {
                    container.removeChild(modal);
                }
            }, 300);
        }
    }
};