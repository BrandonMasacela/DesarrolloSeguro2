class SimpleAlert {
    constructor() {
        this.createContainer();
    }

    createContainer() {
        this.container = document.createElement('div');
        this.container.className = 'simple-alert-container';
        document.body.appendChild(this.container);
    }

    fire(options = {}) {
        const {
            title = '',
            text = '',
            icon = '',
            confirmButtonText = 'OK',
            cancelButtonText = 'Cancelar',
            showCancelButton = false,
            timer = 0
        } = options;

        return new Promise((resolve) => {
            const modal = document.createElement('div');
            modal.className = 'simple-alert-modal';
            
            let iconHtml = '';
            if (icon) {
                iconHtml = `
                    <div class="simple-alert-icon simple-alert-icon-${icon}">
                        ${this.getIconSvg(icon)}
                    </div>
                `;
            }

            modal.innerHTML = `
                <div class="simple-alert-content">
                    ${iconHtml}
                    ${title ? `<h2 class="simple-alert-title">${title}</h2>` : ''}
                    ${text ? `<p class="simple-alert-text">${text}</p>` : ''}
                    <div class="simple-alert-buttons">
                        ${showCancelButton ? 
                            `<button class="simple-alert-button simple-alert-cancel">${cancelButtonText}</button>` : 
                            ''}
                        <button class="simple-alert-button simple-alert-confirm">${confirmButtonText}</button>
                    </div>
                </div>
            `;

            this.container.appendChild(modal);

            // Animación de entrada
            setTimeout(() => modal.classList.add('simple-alert-show'), 10);

            const close = (result) => {
                modal.classList.remove('simple-alert-show');
                setTimeout(() => {
                    this.container.removeChild(modal);
                    resolve(result);
                }, 300);
            };

            // Event listeners
            modal.querySelector('.simple-alert-confirm').addEventListener('click', () => close(true));
            if (showCancelButton) {
                modal.querySelector('.simple-alert-cancel').addEventListener('click', () => close(false));
            }

            // Timer
            if (timer > 0) {
                setTimeout(() => close(true), timer);
            }
        });
    }

    getIconSvg(icon) {
        const icons = {
            success: `
                <svg viewBox="0 0 52 52">
                    <circle cx="26" cy="26" r="25" fill="none" stroke="currentColor" stroke-width="2"/>
                    <path fill="none" stroke="currentColor" stroke-width="2" d="M14.1 27.2l7.1 7.2 16.7-16.8"/>
                </svg>
            `,
            error: `
                <svg viewBox="0 0 52 52">
                    <circle cx="26" cy="26" r="25" fill="none" stroke="currentColor" stroke-width="2"/>
                    <line x1="16" y1="16" x2="36" y2="36" stroke="currentColor" stroke-width="2"/>
                    <line x1="36" y1="16" x2="16" y2="36" stroke="currentColor" stroke-width="2"/>
                </svg>
            `,
            warning: `
                <svg viewBox="0 0 52 52">
                    <circle cx="26" cy="26" r="25" fill="none" stroke="currentColor" stroke-width="2"/>
                    <line x1="26" y1="12" x2="26" y2="32" stroke="currentColor" stroke-width="2"/>
                    <circle cx="26" cy="40" r="2" fill="currentColor"/>
                </svg>
            `,
            info: `
                <svg viewBox="0 0 52 52">
                    <circle cx="26" cy="26" r="25" fill="none" stroke="currentColor" stroke-width="2"/>
                    <circle cx="26" cy="15" r="2" fill="currentColor"/>
                    <line x1="26" y1="22" x2="26" y2="40" stroke="currentColor" stroke-width="2"/>
                </svg>
            `
        };
        return icons[icon] || '';
    }
}

// Crear instancia global
window.Swal = new SimpleAlert(); 