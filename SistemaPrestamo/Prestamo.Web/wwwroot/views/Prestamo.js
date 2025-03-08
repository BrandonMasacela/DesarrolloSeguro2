let tablaData;
let idEditar = 0;
const controlador = "Prestamo";
const modal = "mdData";
const preguntaEliminar = "¿Desea eliminar el préstamo";
const confirmaEliminar = "El préstamo fue eliminado.";
const confirmaRegistro = "Préstamo registrado!";
// Definir la variable token al inicio del script
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
            "url": `/${controlador}/ObtenerPrestamos?IdPrestamo=0&NroDocumento=`,
            "type": "GET",
            'Authorization': `Bearer ${token}`,
            "datatype": "json"
        },
        "columns": [
            { 
                title: "Nro. Prestamo", 
                data: "idPrestamo",
                className: 'col-prestamo fixed-column',
                width: "120px"
            },
            {
                title: "Cliente", 
                data: "cliente",
                className: 'col-cliente',
                width: "200px",
                render: function (data, type, row) {
                    return `${data.nombre} ${data.apellido}`
                }
            },
            { 
                title: "Monto Prestamo", 
                data: "montoPrestamo",
                className: 'col-monto',
                width: "120px"
            },
            { 
                title: "Monto Interes", 
                data: "valorInteres",
                className: 'col-interes',
                width: "120px"
            },
            { 
                title: "Monto Total", 
                data: "valorTotal",
                className: 'col-total',
                width: "120px"
            },
            {
                title: "Moneda", 
                data: "moneda",
                className: 'col-moneda',
                width: "100px",
                render: function (data, type, row) {
                    return `${data.nombre}`
                }
            },
            {
                title: "Estado", 
                data: "estado",
                className: 'col-estado',
                width: "100px",
                render: function (data, type, row) {
                    return data == "Pendiente" ? '<span class="badge bg-danger p-2">Pendiente</span>' : '<span class="badge bg-success p-2">Cancelado</span>'
                }
            },
            {
                title: "Acciones", 
                data: "idPrestamo",
                className: 'col-acciones',
                width: "120px",
                orderable: false,
                render: function (data, type, row) {
                    return `<button class="btn btn-primary btn-action btn-detalle" title="Ver detalle">
                                <i class="fas fa-list-ol"></i>
                            </button>`
                }
            }
        ],
        "order": [[0, 'desc']],
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
});

$("#btnNuevo").on("click", function () {
    window.location.href = '/Prestamo/NuevoPrestamo';
});


$("#tbData tbody").on("click", ".btn-detalle", function () {
    const filaSeleccionada = $(this).closest('tr');
    const prestamo = tablaData.row(filaSeleccionada).data();
    const detalle = prestamo.prestamoDetalle;
    idEditar = prestamo.idPrestamo;
    $("#txtIdPrestamo").text(`Nro. Prestamo: ${prestamo.idPrestamo}`)
    $("#txtMontoPrestamo").val(prestamo.montoPrestamo)
    $("#txtInteres").val(prestamo.interesPorcentaje)
    $("#txtNroCuotas").val(prestamo.nroCuotas)
    $("#txtFormaPago").val(prestamo.formaDePago)
    $("#txtTipoMoneda").val(prestamo.moneda.nombre)
    $("#txtMontoTotal").val(prestamo.valorTotal)

    $("#tbDetalle tbody").html("");

    detalle.forEach(function (e) {
        $("#tbDetalle tbody").append(`<tr>
                                   <td>${e.nroCuota}</td>
                                   <td>${e.fechaPago}</td>
                                   <td>${e.montoCuota}</td>
                                   <td>${e.estado}</td>
                               </tr>`);
    })
    


    $(`#${modal}`).modal('show');
})  

$("#btnImprimir").on("click", function () {
    window.open(`/Prestamo/ImprimirPrestamo?IdPrestamo=${idEditar}`, "_blank");
})