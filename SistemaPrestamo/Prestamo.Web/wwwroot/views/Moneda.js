let tablaData;
let idEditar = 0;
const controlador = "Moneda";
const modal = "mdData";
const preguntaEliminar = "¿Desea eliminar la moneda";
const confirmaEliminar = "La moneda fue eliminada.";
const confirmaRegistro = "Moneda registrada!";
let token;

document.addEventListener("DOMContentLoaded", function (event) {
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
            "headers": {
                'Authorization': `Bearer ${token}`
            },
            "datatype": "json"
        },
        "columns": [
            {
                title: "",
                data: "idMoneda",
                visible: false
            },
            {
                title: "Nombre",
                data: "nombre",
                className: 'col-nombre fixed-column',
                width: "150px"
            },
            {
                title: "Simbolo",
                data: "simbolo",
                className: 'col-simbolo',
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
                data: "idMoneda",
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
        drawCallback: function () {
            $('.dataTables_scrollBody').css('min-height', '300px');
        }
    });
});

$("#tbData tbody").on("click", ".btn-edit", function () {
    var filaSeleccionada = $(this).closest('tr');
    var data = tablaData.row(filaSeleccionada).data();

    idEditar = data.idMoneda;
    $("#txtNombre").val(data.nombre);
    $("#txtSimbolo").val(data.simbolo);
    $(`#${modal}`).modal('show');
    console.log("Editar moneda", data);
});

$("#btnNuevo").on("click", function () {
    idEditar = 0;
    $("#txtNombre").val("");
    $("#txtSimbolo").val("");
    $(`#${modal}`).modal('show');
});

$("#tbData tbody").on("click", ".btn-delete", async function () {
    var filaSeleccionada = $(this).closest('tr');
    var data = tablaData.row(filaSeleccionada).data();
    console.log("Eliminar moneda", data);

    const confirmado = await confirmarAccion(
        `${preguntaEliminar} ${data.nombre}?`,
        "Eliminar Moneda"
    );

    if (confirmado) {
        mostrarCargando("Eliminando moneda...");

        try {
            const response = await fetch(`/${controlador}/Eliminar?Id=${data.idMoneda}`, {
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
                await mostrarMensajeError("No se pudo eliminar la moneda.");
            }
        } catch (error) {
            cerrarAlerta();
            await mostrarMensajeError("Error al intentar eliminar la moneda.");
        }
    }
});

$("#btnGuardar").on("click", async function () {
    const inputs = $(".data-in").serializeArray();
    const inputText = inputs.find((e) => e.value == "");

    if (inputText != undefined) {
        await mostrarMensajeAdvertencia(`Debe completar el campo: ${inputText.name}`);
        return;
    }

    let objeto = {
        idMoneda: idEditar,
        Nombre: $("#txtNombre").val().trim(),
        Simbolo: $("#txtSimbolo").val().trim()
    }

    mostrarCargando(idEditar != 0 ? "Guardando cambios..." : "Registrando moneda...");

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
        await mostrarMensajeError(idEditar != 0 ? "Error al editar la moneda." : "Error al registrar la moneda.");
    }
});