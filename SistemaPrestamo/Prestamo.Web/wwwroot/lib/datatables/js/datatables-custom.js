class SimpleDataTable {
    constructor(tableId, options = {}) {
        this.table = document.getElementById(tableId);
        this.options = {
            pageSize: options.pageSize || 10,
            language: options.language || null,
            ...options
        };
        this.currentPage = 1;
        this.sortColumn = null;
        this.sortDirection = 'asc';
        this.searchTerm = '';
        this.init();
    }

    init() {
        this.wrapTable();
        this.createControls();
        this.loadLanguage();
        this.setupEventListeners();
        this.render();
    }

    wrapTable() {
        const wrapper = document.createElement('div');
        wrapper.className = 'simple-datatable-wrapper';
        this.table.parentNode.insertBefore(wrapper, this.table);
        wrapper.appendChild(this.table);
        this.wrapper = wrapper;
    }

    createControls() {
        // Crear controles de búsqueda
        const searchDiv = document.createElement('div');
        searchDiv.className = 'simple-datatable-search';
        searchDiv.innerHTML = `
            <label>
                ${this.translate('search')}
                <input type="search" class="form-control form-control-sm">
            </label>
        `;
        this.wrapper.insertBefore(searchDiv, this.table);

        // Crear controles de paginación
        const paginationDiv = document.createElement('div');
        paginationDiv.className = 'simple-datatable-pagination';
        this.wrapper.appendChild(paginationDiv);
    }

    async loadLanguage() {
        if (this.options.language) {
            try {
                const response = await fetch(this.options.language);
                this.languageData = await response.json();
            } catch (error) {
                console.error('Error loading language file:', error);
                this.languageData = {};
            }
        }
    }

    translate(key) {
        return this.languageData?.[key] || key;
    }

    setupEventListeners() {
        // Búsqueda
        const searchInput = this.wrapper.querySelector('input[type="search"]');
        searchInput.addEventListener('input', (e) => {
            this.searchTerm = e.target.value.toLowerCase();
            this.currentPage = 1;
            this.render();
        });

        // Ordenamiento
        this.table.querySelectorAll('th').forEach((th, index) => {
            th.style.cursor = 'pointer';
            th.addEventListener('click', () => this.sort(index));
        });
    }

    sort(columnIndex) {
        if (this.sortColumn === columnIndex) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = columnIndex;
            this.sortDirection = 'asc';
        }
        this.render();
    }

    getPageData() {
        let data = Array.from(this.table.querySelectorAll('tbody tr'));

        // Aplicar búsqueda
        if (this.searchTerm) {
            data = data.filter(row => {
                const text = Array.from(row.cells).map(cell => cell.textContent).join(' ').toLowerCase();
                return text.includes(this.searchTerm);
            });
        }

        // Aplicar ordenamiento
        if (this.sortColumn !== null) {
            data.sort((a, b) => {
                const aText = a.cells[this.sortColumn].textContent;
                const bText = b.cells[this.sortColumn].textContent;
                return this.sortDirection === 'asc' ? 
                    aText.localeCompare(bText) : 
                    bText.localeCompare(aText);
            });
        }

        // Calcular paginación
        const start = (this.currentPage - 1) * this.options.pageSize;
        const end = start + this.options.pageSize;
        return {
            total: data.length,
            filtered: data.slice(start, end)
        };
    }

    updatePagination(total) {
        const totalPages = Math.ceil(total / this.options.pageSize);
        const paginationDiv = this.wrapper.querySelector('.simple-datatable-pagination');
        
        let html = '<ul class="pagination">';
        // Botón anterior
        html += `
            <li class="page-item ${this.currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${this.currentPage - 1}">
                    ${this.translate('paginate.previous')}
                </a>
            </li>
        `;

        // Números de página
        for (let i = 1; i <= totalPages; i++) {
            html += `
                <li class="page-item ${this.currentPage === i ? 'active' : ''}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>
            `;
        }

        // Botón siguiente
        html += `
            <li class="page-item ${this.currentPage === totalPages ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${this.currentPage + 1}">
                    ${this.translate('paginate.next')}
                </a>
            </li>
        </ul>`;

        paginationDiv.innerHTML = html;

        // Agregar event listeners
        paginationDiv.querySelectorAll('.page-link').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const page = parseInt(e.target.dataset.page);
                if (page >= 1 && page <= totalPages) {
                    this.currentPage = page;
                    this.render();
                }
            });
        });
    }

    render() {
        const { total, filtered } = this.getPageData();
        
        // Ocultar todas las filas
        this.table.querySelectorAll('tbody tr').forEach(tr => tr.style.display = 'none');
        
        // Mostrar solo las filas filtradas
        filtered.forEach(tr => tr.style.display = '');
        
        // Actualizar paginación
        this.updatePagination(total);
    }
}

// Agregar al objeto window
window.SimpleDataTable = SimpleDataTable; 