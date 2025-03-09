let tablaData;
let idEditar = 0;
const controlador = "Cliente";
const modal = "mdData";
const preguntaEliminar = "¿Desea eliminar el cliente";
const confirmaEliminar = "El cliente fue eliminado.";
const confirmaRegistro = "Cliente registrado!";
let token;

document.addEventListener('DOMContentLoaded', function () {
    token = localStorage.getItem('token');

    if (!token) {
        $.LoadingOverlay("hide");
        mostrarMensajeAdvertencia("No se encontró el token de autenticación.");
        return;
    }

    tablaData = $('#tbData').DataTable({
        responsive: true,
        scrollX: true,
        scrollY: '50vh',
        scrollCollapse: true,
        autoWidth: false,
        "ajax": {
            "url": `/${controlador}/Lista`,
            "type": "GET",
            'Authorization': `Bearer ${token}`,
            "datatype": "json"
        },
        "columns": [
            { 
                title: "", 
                data: "idCliente", 
                visible: false 
            },
            { 
                title: "Nro. Documento", 
                data: "nroDocumento",
                className: 'col-documento fixed-column',
                width: "120px"
            },
            { 
                title: "Nombre", 
                data: "nombre",
                className: 'col-nombre',
                width: "150px"
            },
            { 
                title: "Apellido", 
                data: "apellido",
                className: 'col-apellido',
                width: "150px"
            },
            { 
                title: "Correo", 
                data: "correo",
                className: 'col-correo',
                width: "200px"
            },
            { 
                title: "Teléfono", 
                data: "telefono",
                className: 'col-telefono',
                width: "120px"
            },
            { 
                title: "Fecha Creación", 
                data: "fechaCreacion",
                className: 'col-fecha',
                width: "120px"
            },
            { 
                title: "Acciones", 
                data: "idCliente",
                className: 'col-acciones',
                width: "100px",
                orderable: false,
                render: function (data, type, row) {
                    return `<button class="btn btn-primary btn-action btn-edit" title="Editar">
                                <i class="fas fa-pen"></i>
                            </button>
                            <button class="btn btn-danger btn-action btn-delete" title="Eliminar">
                                <i class="fas fa-trash"></i>
                            </button>`;
                }
            }
        ],
        "order": [[1, 'asc']],
        fixedColumns: {
            left: 1
        },
        language: {
            search: "Buscar:",
            lengthMenu: "Mostrar _MENU_ registros por página",
            info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
            infoEmpty: "Mostrando 0 a 0 de 0 registros",
            infoFiltered: "(filtrado de _MAX_ registros totales)",
            zeroRecords: "No se encontraron resultados",
            emptyTable: "Ningún dato disponible en esta tabla",
            paginate: {
                first: "Primero",
                last: "Último",
                next: "Siguiente",
                previous: "Anterior"
            }
        },
        dom: '<"row"<"col-sm-12 col-md-6"f><"col-sm-12 col-md-6"l>>' +
             '<"row"<"col-sm-12"tr>>' +
             '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        drawCallback: function() {
            $('.dataTables_scrollBody').css('min-height', '300px');
        }
    });

    // Validaciones de entrada
    $("#txtNroDocumento").on("input", function() {
        // Solo permitir números
        this.value = this.value.replace(/[^0-9]/g, '');
    });

    $("#txtNombre, #txtApellido").on("input", function() {
        // Solo permitir letras, espacios y tildes
        this.value = this.value.replace(/[^a-záéíóúñA-ZÁÉÍÓÚÑ\s]/g, '');
    });

    $("#txtTelefono").on("input", function() {
        // Solo permitir números
        this.value = this.value.replace(/[^0-9]/g, '');
    });
});

$("#tbData tbody").on("click", ".btn-edit", function () {
    var filaSeleccionada = $(this).closest('tr');
    var data = tablaData.row(filaSeleccionada).data();

    idEditar = data.idCliente;
    $("#txtNroDocumento").val(data.nroDocumento);
    $("#txtNroDocumento").attr("disabled", true); 
    $("#txtNombre").val(data.nombre);
    $("#txtApellido").val(data.apellido);
    $("#txtCorreo").val(data.correo);
    $("#txtTelefono").val(data.telefono);
    $(`#${modal}`).modal('show');
});

$("#btnNuevo").on("click", function () {
    idEditar = 0;
    $("#txtNroDocumento").val("");
    $("#txtNroDocumento").attr("disabled", false);
    $("#txtNombre").val("");
    $("#txtApellido").val("");
    $("#txtCorreo").val("");
    $("#txtTelefono").val("");
    $(`#${modal}`).modal('show');
});

$("#tbData tbody").on("click", ".btn-delete", async function () {
    var filaSeleccionada = $(this).closest('tr');
    var data = tablaData.row(filaSeleccionada).data();

    const confirmado = await confirmarAccion(
        `${preguntaEliminar} ${data.nombre} ${data.apellido}?`,
        "Eliminar Cliente"
    );

    if (confirmado) {
        mostrarCargando("Eliminando cliente...");
        
        try {
            const response = await fetch(`/${controlador}/Eliminar?Id=${data.idCliente}`, {
                method: "DELETE",
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json;charset=utf-8'
                }
            });

            const responseJson = await response.json();
            cerrarAlerta();

            if (responseJson.data == "") {
                await mostrarMensajeExito(confirmaEliminar);
                tablaData.ajax.reload();
            } else {
                await mostrarMensajeError("No se pudo eliminar el cliente.");
            }
        } catch (error) {
            cerrarAlerta();
            await mostrarMensajeError("Error al intentar eliminar el cliente.");
        }
    }
});

$("#btnGuardar").on("click", async function () {
    // Validaciones adicionales antes de guardar
    const nroDocumento = $("#txtNroDocumento").val().trim();
    const nombre = $("#txtNombre").val().trim();
    const apellido = $("#txtApellido").val().trim();
    const correo = $("#txtCorreo").val().trim();
    const telefono = $("#txtTelefono").val().trim();

    // Validar documento
    if (nroDocumento.length < 8) {
        await mostrarMensajeAdvertencia("El número de documento debe tener al menos 8 dígitos");
        $("#txtNroDocumento").focus();
        return;
    }

    // Validar nombre
    if (nombre.length < 2) {
        await mostrarMensajeAdvertencia("El nombre debe tener al menos 2 caracteres");
        $("#txtNombre").focus();
        return;
    }

    // Validar apellido
    if (apellido.length < 2) {
        await mostrarMensajeAdvertencia("El apellido debe tener al menos 2 caracteres");
        $("#txtApellido").focus();
        return;
    }

    // Validar correo
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!emailRegex.test(correo)) {
        await mostrarMensajeAdvertencia("Por favor ingrese un correo electrónico válido");
        $("#txtCorreo").focus();
        return;
    }

    // Validar teléfono
    if (telefono.length < 9) {
        await mostrarMensajeAdvertencia("El teléfono debe tener al menos 9 dígitos");
        $("#txtTelefono").focus();
        return;
    }

    let objeto = {
        idCliente: idEditar,
        NroDocumento: nroDocumento,
        Nombre: nombre,
        Apellido: apellido,
        Correo: correo,
        Telefono: telefono
    }

    mostrarCargando(idEditar != 0 ? "Guardando cambios..." : "Registrando cliente...");

    try {
        const response = await fetch(`/${controlador}/${idEditar != 0 ? 'Editar' : 'Crear'}`, {
            method: idEditar != 0 ? "PUT" : "POST",
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(objeto)
        });

        const responseJson = await response.json();
        cerrarAlerta();

        if (responseJson.data == "") {
            await mostrarMensajeExito(idEditar != 0 ? "Se guardaron los cambios!" : confirmaRegistro);
            idEditar = 0;
            $(`#${modal}`).modal('hide');
            tablaData.ajax.reload();
        } else {
            await mostrarMensajeError(responseJson.data);
        }
    } catch (error) {
        cerrarAlerta();
        await mostrarMensajeError(idEditar != 0 ? "Error al editar el cliente." : "Error al registrar el cliente.");
    }
});