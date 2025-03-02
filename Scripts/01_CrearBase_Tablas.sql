create database DBPrestamo

go

use DBPrestamo

go

create table Cliente(
IdCliente int primary key identity,
NroDocumento varchar(50),
Nombre varchar(50),
Apellido varchar(50),
Correo  varchar(50),
Telefono  varchar(50),
FechaCreacion datetime default getdate()
)

go

create table Moneda(
IdMoneda int primary key identity,
Nombre varchar(50),
Simbolo varchar(4),
FechaCreacion datetime default getdate()
)

go

create table Cuenta (
    IdCuenta int primary key identity,
    IdCliente int references Cliente(IdCliente),
    Tarjeta varchar(64),
    FechaCreacion datetime default getdate(),
    Monto decimal(18, 2)
)

go

create table Prestamo(
IdPrestamo int primary key identity,
IdCliente int,
IdMoneda int,
FechaInicioPago date,
MontoPrestamo decimal(10,2),
InteresPorcentaje decimal(10,2),
NroCuotas int,
FormaDePago varchar(50),--Diario,Semanal,Quincenal,Mensual
ValorPorCuota decimal(10,2),
ValorInteres decimal(10,2),
ValorTotal decimal(10,2),
Estado varchar(50),--Pendiente,Cancelado
FechaCreacion datetime default getdate()
)

go

create table PrestamoDetalle(
IdPrestamoDetalle int primary key identity,
IdPrestamo int references Prestamo(IdPrestamo),
FechaPago date,
NroCuota int,
MontoCuota decimal(10,2),
Estado varchar(50),--Pendiente,Cancelado
FechaPagado datetime ,
FechaCreacion datetime default getdate()
)

go

create table Usuario(
IdUsuario int primary key identity,
NombreCompleto varchar(50),
Correo varchar(50),
Clave varchar(60),
Rol varchar(50),
FailedAttempts INT DEFAULT 0,
LastFailedAttempt datetime NULL default getdate(),
LockoutEnd datetime NULL default  getdate(),
IsLocked BIT DEFAULT 0,
FechaCreacion datetime default getdate()
)

go

CREATE TABLE SolicitudPrestamo
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,
    Monto DECIMAL(18, 2) NOT NULL,
    Plazo INT NOT NULL,
    Estado NVARCHAR(50) NOT NULL,
    FechaSolicitud DATETIME NOT NULL,
	Sueldo DECIMAL(18, 2) NOT NULL,
    EsCasado BIT NOT NULL,
    NumeroHijos INT NOT NULL,
    MetodoPago NVARCHAR(50) NOT NULL,
	Cedula NVARCHAR(10) NOT NULL,
    Ocupacion NVARCHAR(100) NOT NULL
)
go


CREATE TABLE HistorialCrediticio
(
    IdUsuario INT PRIMARY KEY,
    EstadoCrediticio INT NOT NULL
);


CREATE TABLE Auditoria (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Usuario NVARCHAR(256),
    Accion NVARCHAR(256),
    Fecha DATETIME,
    Detalles NVARCHAR(MAX)
);
