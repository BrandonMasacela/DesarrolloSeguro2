USE DBPrestamo
GO

--- FUNCIÓN SPLITSTRING ---
CREATE FUNCTION [dbo].[SplitString] ( 
    @string NVARCHAR(MAX), 
    @delimiter CHAR(1)  
)
RETURNS @output TABLE(valor NVARCHAR(MAX))
BEGIN 
    DECLARE @start INT, @end INT 
    SELECT @start = 1, @end = CHARINDEX(@delimiter, @string) 
    WHILE @start < LEN(@string) + 1 BEGIN 
        IF @end = 0 SET @end = LEN(@string) + 1 
        INSERT INTO @output VALUES(SUBSTRING(@string, @start, @end - @start)) 
        SET @start = @end + 1 
        SET @end = CHARINDEX(@delimiter, @string, @start) 
    END 
    RETURN
END
GO

--- PROCEDIMIENTOS DE USUARIO ---
CREATE PROCEDURE [dbo].[sp_crearUsuario]
    @NombreCompleto NVARCHAR(100),
    @Correo NVARCHAR(100),
    @Clave NVARCHAR(60),
    @Rol NVARCHAR(50)
AS
BEGIN
    INSERT INTO Usuario (NombreCompleto, Correo, Clave, FechaCreacion, Rol)
    VALUES (@NombreCompleto, @Correo, @Clave, GETDATE(), @Rol)
END
GO

CREATE PROCEDURE [dbo].[sp_obtenerUsuario]
    @Correo VARCHAR(50),
    @Clave VARCHAR(50)
AS
BEGIN
    SELECT 
        IdUsuario,
        NombreCompleto,
        Correo 
    FROM Usuario 
    WHERE 
        Correo = @Correo COLLATE SQL_Latin1_General_CP1_CS_AS AND
        Clave = @Clave COLLATE SQL_Latin1_General_CP1_CS_AS
END
GO

CREATE PROCEDURE [dbo].[sp_obtenerUsuarioPorCorreo]
@Correo VARCHAR(100)
AS
BEGIN
    SELECT 
        IdUsuario,
        NombreCompleto,
        Correo,
        Clave,
        Rol,
        FailedAttempts,
        IsLocked,
		LockoutEnd
    FROM Usuario
    WHERE Correo = @Correo
END
GO

--- PROCEDIMIENTOS DE MONEDA ---
CREATE PROCEDURE [dbo].[sp_listaMoneda]
AS
BEGIN
    SELECT 
        IdMoneda,
        Nombre,
        Simbolo,
        CONVERT(CHAR(10), FechaCreacion, 103) [FechaCreacion] 
    FROM Moneda
END
GO

CREATE PROCEDURE [dbo].[sp_crearMoneda]
    @Nombre VARCHAR(50),
    @Simbolo VARCHAR(50),
    @msgError VARCHAR(100) OUTPUT
AS
BEGIN
    SET @msgError = ''
    IF NOT EXISTS(SELECT * FROM Moneda WHERE Nombre = @Nombre COLLATE SQL_Latin1_General_CP1_CS_AS)
        INSERT INTO Moneda(Nombre, Simbolo) 
        VALUES(@Nombre, @Simbolo)
    ELSE
        SET @msgError = 'La moneda ya existe'
END
GO

CREATE PROCEDURE [dbo].[sp_editarMoneda]
    @IdMoneda INT,
    @Nombre VARCHAR(50),
    @Simbolo VARCHAR(50),
    @msgError VARCHAR(100) OUTPUT
AS
BEGIN
    SET @msgError = ''
    IF NOT EXISTS(SELECT * FROM Moneda WHERE Nombre = @Nombre COLLATE SQL_Latin1_General_CP1_CS_AS AND IdMoneda != @IdMoneda)
        UPDATE Moneda 
        SET Nombre = @Nombre, Simbolo = @Simbolo 
        WHERE IdMoneda = @IdMoneda
    ELSE
        SET @msgError = 'La moneda ya existe'
END
GO

CREATE PROCEDURE [dbo].[sp_eliminarMoneda]
    @IdMoneda INT,
    @msgError VARCHAR(100) OUTPUT
AS
BEGIN
    SET @msgError = ''
    IF NOT EXISTS(SELECT IdPrestamo FROM Prestamo WHERE IdMoneda = @IdMoneda)
        DELETE FROM Moneda 
        WHERE IdMoneda = @IdMoneda
    ELSE
        SET @msgError = 'La moneda está utilizada en un préstamo'
END
GO

--- PROCEDIMIENTOS DE CLIENTE ---
CREATE PROCEDURE [dbo].[sp_listaCliente]
AS
BEGIN
    SELECT 
        IdCliente,
        NroDocumento,
        Nombre,
        Apellido,
        Correo,
        Telefono,
        CONVERT(CHAR(10), FechaCreacion, 103) [FechaCreacion] 
    FROM Cliente
END
GO

CREATE PROCEDURE [dbo].[sp_obtenerCliente]
    @NroDocumento VARCHAR(50)
AS
BEGIN
    SELECT 
        IdCliente,
        NroDocumento,
        Nombre,
        Apellido,
        Correo,
        Telefono,
        CONVERT(CHAR(10), FechaCreacion, 103) [FechaCreacion] 
    FROM Cliente 
    WHERE NroDocumento = @NroDocumento
END
GO

CREATE PROCEDURE [dbo].[sp_crearCliente]
    @NroDocumento VARCHAR(50),
    @Nombre VARCHAR(50),
    @Apellido VARCHAR(50),
    @Correo VARCHAR(50),
    @Telefono VARCHAR(50),
    @IdCliente INT OUTPUT,
    @msgError VARCHAR(100) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @msgError = '';
    BEGIN TRY
        IF NOT EXISTS(SELECT * FROM Cliente WHERE NroDocumento = @NroDocumento) BEGIN
            INSERT INTO Cliente (NroDocumento, Nombre, Apellido, Correo, Telefono, FechaCreacion)
            VALUES (@NroDocumento, @Nombre, @Apellido, @Correo, @Telefono, GETDATE())
            SET @IdCliente = SCOPE_IDENTITY();
        END ELSE
            SET @msgError = 'El cliente ya existe'
    END TRY
    BEGIN CATCH
        SET @msgError = ERROR_MESSAGE()
    END CATCH
END
GO

CREATE PROCEDURE [dbo].[sp_editarCliente]
    @IdCliente INT,
    @NroDocumento VARCHAR(50),
    @Nombre VARCHAR(50),
    @Apellido VARCHAR(50),
    @Correo VARCHAR(50),
    @Telefono VARCHAR(50),
    @msgError VARCHAR(100) OUTPUT
AS
BEGIN
    SET @msgError = ''
    IF NOT EXISTS(SELECT * FROM Cliente WHERE NroDocumento = @NroDocumento AND IdCliente != @IdCliente)
        UPDATE Cliente 
        SET 
            NroDocumento = @NroDocumento,
            Nombre = @Nombre,
            Apellido = @Apellido,
            Correo = @Correo,
            Telefono = @Telefono 
        WHERE IdCliente = @IdCliente
    ELSE
        SET @msgError = 'El cliente ya existe'
END
GO

CREATE PROCEDURE [dbo].[sp_eliminarCliente]
    @IdCliente INT,
    @msgError VARCHAR(100) OUTPUT
AS
BEGIN
    SET @msgError = ''
    IF NOT EXISTS(SELECT IdPrestamo FROM Prestamo WHERE IdCliente = @IdCliente)
        DELETE FROM Cliente 
        WHERE IdCliente = @IdCliente
    ELSE
        SET @msgError = 'El cliente tiene préstamos asociados'
END
GO

CREATE PROCEDURE [dbo].[sp_obtenerClientePorCorreo]
    @Correo NVARCHAR(100)
AS
BEGIN
    SELECT 
        IdCliente,
        NroDocumento,
        Nombre,
        Apellido,
        Correo,
        Telefono,
        FechaCreacion
    FROM Cliente 
    WHERE Correo = @Correo
END
GO

--- PROCEDIMIENTOS DE PRÉSTAMOS ---
CREATE PROCEDURE [dbo].[sp_crearPrestamo]
    @IdCliente INT,
    @NroDocumento VARCHAR(50),
    @Nombre VARCHAR(50),
    @Apellido VARCHAR(50),
    @Correo VARCHAR(50),
    @Telefono VARCHAR(50),
    @IdMoneda INT,
    @FechaInicio VARCHAR(50),
    @MontoPrestamo VARCHAR(50),
    @InteresPorcentaje VARCHAR(50),
    @NroCuotas INT,
    @FormaDePago VARCHAR(50),
    @ValorPorCuota VARCHAR(50),
    @ValorInteres VARCHAR(50),
    @ValorTotal VARCHAR(50),
    @msgError VARCHAR(100) OUTPUT
AS
BEGIN
    set dateformat dmy
	set @msgError = ''

	begin try

		declare @FecInicio date = convert(date,@FechaInicio)
		declare @MontPrestamo decimal(10,2) = convert(decimal(10,2),@MontoPrestamo)
		declare @IntPorcentaje decimal(10,2) = convert(decimal(10,2),@InteresPorcentaje)
		declare @VlrPorCuota decimal(10,2) = convert(decimal(10,2),@ValorPorCuota)
		declare @VlrInteres decimal(10,2) = convert(decimal(10,2),@ValorInteres)
		declare @VlrTotal decimal(10,2) = convert(decimal(10,2),@ValorTotal)
		create table #TempIdentity(Id int,Nombre varchar(10))

		begin transaction

		if(@IdCliente = 0)
		begin
			insert into Cliente(NroDocumento,Nombre,Apellido,Correo,Telefono)
			OUTPUT INSERTED.IdCliente,'Cliente' INTO #TempIdentity(Id,Nombre)
			values
			(@NroDocumento,@Nombre,@Apellido,@Correo,@Telefono)

			set @IdCliente = (select Id from #TempIdentity where Nombre = 'Cliente')
		end
		else
		begin
			if(exists(select * from Prestamo where IdCliente = @IdCliente and Estado = 'Pendiente'))
				set @msgError = 'El cliente tiene un prestamo pendiente, debe cancelar el anterior'
		end

		if(@msgError ='')
		begin

			insert into Prestamo(IdCliente,IdMoneda,FechaInicioPago,MontoPrestamo,InteresPorcentaje,NroCuotas,FormaDePago,ValorPorCuota,ValorInteres,ValorTotal,Estado)
			OUTPUT INSERTED.IdPrestamo,'Prestamo' INTO #TempIdentity(Id,Nombre)
			values
			(@IdCliente,@IdMoneda,@FecInicio,@MontPrestamo,@IntPorcentaje,@NroCuotas,@FormaDePago,@VlrPorCuota,@VlrInteres,@VlrTotal,'Pendiente')

			;with detalle(IdPrestamo,FechaPago,NroCuota,MontoCuota,Estado) as
			(
				select (select Id from #TempIdentity where Nombre = 'Prestamo'),@FecInicio,0,@VlrPorCuota,'Pendiente'
				union all
				select IdPrestamo,
				case @FormaDePago 
					when 'Diario' then DATEADD(day,1,FechaPago)
					when 'Semanal' then DATEADD(WEEK,1,FechaPago)
					when 'Quincenal' then DATEADD(day,15,FechaPago)
					when 'Mensual' then DATEADD(MONTH,1,FechaPago)
				end,
				NroCuota + 1,MontoCuota,Estado from detalle
				where NroCuota < @NroCuotas
			)
			select IdPrestamo,FechaPago,NroCuota,MontoCuota,Estado into #tempDetalle from detalle where NroCuota > 0
	
			insert into PrestamoDetalle(IdPrestamo,FechaPago,NroCuota,MontoCuota,Estado)
			select IdPrestamo,FechaPago,NroCuota,MontoCuota,Estado from #tempDetalle

		end

		commit transaction
	end try
	begin catch
		rollback transaction
		set @msgError = ERROR_MESSAGE()
	end catch
END
GO

CREATE PROCEDURE [dbo].[sp_obtenerPrestamos]
    @IdPrestamo INT = 0,
    @NroDocumento VARCHAR(50) = ''
AS
BEGIN
    SELECT TOP 1
        p.IdPrestamo,
        c.IdCliente,
        c.NroDocumento,
        c.Nombre,
        c.Apellido,
        c.Correo,
        c.Telefono,
        m.IdMoneda,
        m.Nombre AS [NombreMoneda],
        m.Simbolo,
        CONVERT(char(10), p.FechaInicioPago, 103) AS [FechaInicioPago],
        CONVERT(VARCHAR, p.MontoPrestamo) AS [MontoPrestamo],
        CONVERT(VARCHAR, p.InteresPorcentaje) AS [InteresPorcentaje],
        p.NroCuotas,
        p.FormaDePago,
        CONVERT(VARCHAR, p.ValorPorCuota) AS [ValorPorCuota],
        CONVERT(VARCHAR, p.ValorInteres) AS [ValorInteres],
        CONVERT(VARCHAR, p.ValorTotal) AS [ValorTotal],
        p.Estado,
        CONVERT(char(10), p.FechaCreacion, 103) AS [FechaCreacion],
        (
            SELECT
                pd.IdPrestamoDetalle,
                CONVERT(char(10), pd.FechaPago, 103) AS [FechaPago],
                CONVERT(VARCHAR, pd.MontoCuota) AS [MontoCuota],
                pd.NroCuota,
                pd.Estado,
                ISNULL(CONVERT(varchar(10), pd.FechaPagado, 103), '') AS [FechaPagado]
            FROM PrestamoDetalle pd
            WHERE pd.IdPrestamo = p.IdPrestamo
            FOR XML PATH('Detalle'), TYPE, ROOT('PrestamoDetalle')
        )
    FROM Prestamo p
    INNER JOIN Cliente c ON c.IdCliente = p.IdCliente
    INNER JOIN Moneda m ON m.IdMoneda = p.IdMoneda
    WHERE p.IdPrestamo = IIF(@IdPrestamo = 0, p.IdPrestamo, @IdPrestamo)
      AND c.NroDocumento = IIF(@NroDocumento = '', c.NroDocumento, @NroDocumento)
    ORDER BY p.FechaCreacion DESC
    FOR XML PATH('Prestamo'), ROOT('Prestamos'), TYPE;
END
GO

CREATE PROCEDURE [dbo].[sp_pagarCuotas]
    @IdPrestamo INT,
    @NroCuotasPagadas VARCHAR(100),
    @NumeroTarjeta VARCHAR(16),
    @msgError VARCHAR(100) OUTPUT
AS
BEGIN
    SET DATEFORMAT dmy;
    SET @msgError = '';

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @IdCliente INT;
        DECLARE @TotalPagar DECIMAL(18, 2);

        -- Obtener el IdCliente y el total a pagar
        SELECT @IdCliente = p.IdCliente,
               @TotalPagar = SUM(pd.MontoCuota)
        FROM Prestamo p
        INNER JOIN PrestamoDetalle pd ON p.IdPrestamo = pd.IdPrestamo
        INNER JOIN dbo.SplitString(@NroCuotasPagadas, ',') ss ON ss.valor = pd.NroCuota
        WHERE p.IdPrestamo = @IdPrestamo
        GROUP BY p.IdCliente;

        -- Verificar el saldo de la cuenta
        DECLARE @SaldoCuenta DECIMAL(18, 2);
        DECLARE @Tarjeta NVARCHAR(64);
        DECLARE @TarjetaDesencriptada NVARCHAR(16);

        -- Obtener el saldo y la tarjeta encriptada
        SELECT @SaldoCuenta = c.Monto, 
               @Tarjeta = c.Tarjeta
        FROM Cuenta c
        WHERE c.IdCliente = @IdCliente;

        -- Desencriptar la tarjeta antes de compararla
        SET @TarjetaDesencriptada = CONVERT(NVARCHAR(16), DecryptByPassphrase('aB3dE5fG7hI9jK1mN2oP4qR6sT8uV0wX', @Tarjeta));

        -- Validar que la tarjeta ingresada coincida con la desencriptada
        IF @TarjetaDesencriptada != @NumeroTarjeta
        BEGIN
            SET @msgError = 'Número de tarjeta incorrecto';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Verificar si hay saldo suficiente
        IF @SaldoCuenta < @TotalPagar
        BEGIN
            SET @msgError = 'Fondos insuficientes' + @TarjetaDesencriptada;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Actualizar el saldo de la cuenta
        UPDATE Cuenta
        SET Monto = Monto - @TotalPagar
        WHERE IdCliente = @IdCliente;

        -- Actualizar el estado de las cuotas
        UPDATE pd
        SET pd.Estado = 'Cancelado', FechaPagado = GETDATE()
        FROM PrestamoDetalle pd
        INNER JOIN dbo.SplitString(@NroCuotasPagadas, ',') ss ON ss.valor = pd.NroCuota
        WHERE IdPrestamo = @IdPrestamo;

        -- Actualizar el estado del préstamo si todas las cuotas están pagadas
        IF (SELECT COUNT(IdPrestamoDetalle) FROM PrestamoDetalle WHERE IdPrestamo = @IdPrestamo AND Estado = 'Pendiente') = 0
        BEGIN
            UPDATE Prestamo
            SET Estado = 'Cancelado'
            WHERE IdPrestamo = @IdPrestamo;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
		SET @msgError = 'Error en sp_pagarCuotas: ' + ERROR_MESSAGE() + ' | IdPrestamo: ' + CAST(@IdPrestamo AS VARCHAR) + ' | NroCuotasPagadas: ' + @NroCuotasPagadas + ' | NumeroTarjeta: ' + @NumeroTarjeta;
    END CATCH;
END
GO

--- PROCEDIMIENTOS DE CUENTAS ---
CREATE PROCEDURE [dbo].[sp_obtenerCuenta]
    @IdCliente INT
AS
BEGIN
    SELECT 
        IdCuenta, 
        IdCliente, 
        Tarjeta, 
        Monto 
    FROM Cuenta 
    WHERE IdCliente = @IdCliente
END
GO

CREATE PROCEDURE [dbo].[sp_depositarCuenta]
    @IdCliente INT,
    @Monto DECIMAL(18,2),
    @msgError VARCHAR(100) OUTPUT
AS
BEGIN
    SET @msgError = ''
    
    BEGIN TRY
        BEGIN TRANSACTION
            
            IF NOT EXISTS (SELECT 1 FROM Cuenta WHERE IdCliente = @IdCliente)
            BEGIN
                SET @msgError = 'No se encontró la cuenta del cliente'
                ROLLBACK
                RETURN
            END
            
            IF @Monto <= 0
            BEGIN
                SET @msgError = 'El monto debe ser mayor a 0'
                ROLLBACK
                RETURN
            END
            
            UPDATE Cuenta
            SET Monto = Monto + @Monto
            WHERE IdCliente = @IdCliente
            
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        SET @msgError = ERROR_MESSAGE()
        ROLLBACK TRANSACTION
    END CATCH
END
GO

CREATE PROCEDURE [dbo].[sp_crearCuenta]
    @IdCliente INT,
    @Tarjeta VARCHAR(64),
    @Monto DECIMAL(18, 2),
    @msgError NVARCHAR(100) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Insertar la nueva cuenta en la tabla Cuenta
        INSERT INTO Cuenta (IdCliente, Tarjeta, Monto, FechaCreacion)
        VALUES (@IdCliente, @Tarjeta, @Monto, GETDATE());

        -- Establecer el mensaje de error a vacío si la operación es exitosa
        SET @msgError = '';
    END TRY
    BEGIN CATCH
        -- Capturar el error y establecer el mensaje de error
        SET @msgError = ERROR_MESSAGE();
    END CATCH
END
GO

--- PROCEDIMIENTOS DE CONTROL DE LOGIN ---
CREATE PROCEDURE sp_UpdateLockoutStatus
    @IdUsuario INT,
    @FailedAttempts INT,
    @LastFailedAttempt DATETIME,
    @LockoutEnd DATETIME = NULL,
    @IsLocked BIT
AS
BEGIN
    UPDATE Usuario
    SET FailedAttempts = @FailedAttempts,
        LastFailedAttempt = @LastFailedAttempt,
        LockoutEnd = @LockoutEnd,
        IsLocked = @IsLocked
    WHERE IdUsuario = @IdUsuario
END
GO

-- Procedimiento almacenado para resetear el bloqueo
CREATE PROCEDURE sp_ResetLockout
    @IdUsuario INT
AS
BEGIN
    UPDATE Usuario
    SET FailedAttempts = 0,
        LastFailedAttempt = NULL,
        LockoutEnd = NULL,
        IsLocked = 0
    WHERE IdUsuario = @IdUsuario
END
GO

-- Procedimiento para cambio de contraseña
CREATE PROCEDURE sp_actualizarUsuario
    @IdUsuario INT,
    @Clave NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Usuario
    SET Clave = @Clave
    WHERE IdUsuario = @IdUsuario;
END
GO

-- Procedimiento para obtener usuario por id
CREATE PROCEDURE [dbo].[sp_obtenerUsuarioPorId]
@IdUsuario INT
AS
BEGIN
    SELECT
		IdUsuario,
        NombreCompleto,
        Correo,
        Clave,
        Rol,
        FailedAttempts,
        IsLocked,
		LockoutEnd
    FROM Usuario
    WHERE IdUsuario = @IdUsuario;
END
GO


--- PROCEDIMIENTOS DE SOLICITUD PRESTAMO---

CREATE PROCEDURE sp_crearSolicitudPrestamo
    @IdUsuario INT,
    @Monto DECIMAL(18, 2),
    @Plazo INT,
    @Estado NVARCHAR(50),
    @FechaSolicitud DATETIME,
    @Sueldo DECIMAL(18, 2),
    @EsCasado BIT,
    @NumeroHijos INT,
    @MetodoPago NVARCHAR(50),
	@Cedula NVARCHAR(10),
    @Ocupacion NVARCHAR(100)
AS
BEGIN
    INSERT INTO SolicitudPrestamo (IdUsuario, Monto, Plazo, Estado, FechaSolicitud, Sueldo, EsCasado, NumeroHijos, MetodoPago, Cedula, Ocupacion)
    VALUES (@IdUsuario, @Monto, @Plazo, @Estado, @FechaSolicitud, @Sueldo, @EsCasado, @NumeroHijos, @MetodoPago, @Cedula, @Ocupacion);
END
GO

CREATE PROCEDURE sp_obtenerSolicitudesPendientes
AS
BEGIN
    SELECT Id, IdUsuario, Monto, Plazo, Estado, FechaSolicitud, Sueldo, EsCasado, NumeroHijos, MetodoPago, Cedula, Ocupacion
    FROM SolicitudPrestamo
    WHERE Estado = 'Pendiente';
END
GO

CREATE PROCEDURE sp_actualizarEstadoSolicitud
    @Id INT,
    @Estado NVARCHAR(50)
AS
BEGIN
    UPDATE SolicitudPrestamo
    SET Estado = @Estado
    WHERE Id = @Id;
END
GO

CREATE PROCEDURE sp_obtenerHistorialCrediticio
    @IdUsuario INT
AS
BEGIN
    SELECT IdUsuario, EstadoCrediticio
    FROM HistorialCrediticio
    WHERE IdUsuario = @IdUsuario;
END
GO

CREATE PROCEDURE sp_crearHistorialCrediticio
    @IdUsuario INT,
    @EstadoCrediticio INT
AS
BEGIN
    INSERT INTO HistorialCrediticio (IdUsuario, EstadoCrediticio)
    VALUES (@IdUsuario, @EstadoCrediticio);
END
GO

CREATE PROCEDURE sp_actualizarHistorialCrediticio
    @IdUsuario INT,
    @Aprobado BIT
AS
BEGIN
    IF @Aprobado = 1
    BEGIN
        UPDATE HistorialCrediticio
        SET EstadoCrediticio = EstadoCrediticio + 1
        WHERE IdUsuario = @IdUsuario;
    END
    ELSE
    BEGIN
        UPDATE HistorialCrediticio
        SET EstadoCrediticio = EstadoCrediticio - 1
        WHERE IdUsuario = @IdUsuario;
    END
END
GO

--- PROCEDIMIENTOS DE REPORTES CLIENTES ---
CREATE PROCEDURE [dbo].[sp_obtenerResumenPorCliente]
    @IdCliente INT,
    @IdPrestamo INT
AS
BEGIN
    DECLARE @PrestamosPendientes INT;
    DECLARE @PrestamosCancelados INT;
	DECLARE @PrestamosTotales INT;

	SELECT @PrestamosTotales = COUNT(*) 
    FROM Prestamo 
    WHERE IdCliente = @IdCliente AND Estado = 'Pendiente';
    -- Obtener cantidad de préstamos pendientes del cliente
    SELECT @PrestamosPendientes = COUNT(*) 
    FROM PrestamoDetalle  
    WHERE IdPrestamo = @IdPrestamo AND Estado = 'Pendiente';

    -- Obtener cantidad de préstamos cancelados del préstamo específico
    SELECT @PrestamosCancelados = COUNT(*) 
    FROM PrestamoDetalle 
    WHERE IdPrestamo = @IdPrestamo AND Estado = 'Cancelado';

    -- Retornar resultados 
    SELECT 
        @PrestamosPendientes AS [PrestamosPendientes],
        @PrestamosTotales AS [PrestamosPagados];
END
GO


CREATE PROCEDURE sp_obtenerIdPrestamoPorCliente
    @IdCliente INT
AS
BEGIN
    SELECT TOP 1 IdPrestamo
    FROM Prestamo
    WHERE IdCliente = @IdCliente
    ORDER BY FechaCreacion DESC;
END
GO

CREATE PROCEDURE sp_obtenerSolicitudPorId
    @Id INT
AS
BEGIN
    SELECT Id, IdUsuario, Monto, Plazo, Estado, FechaSolicitud, Sueldo, EsCasado, NumeroHijos, MetodoPago, Cedula, Ocupacion
    FROM SolicitudPrestamo
    WHERE Id = @Id;
END
GO

--- PROCEDIMIENTOS DE REPORTES ---
CREATE PROCEDURE [dbo].[sp_obtenerResumen]
AS
BEGIN
    SELECT 
        (SELECT CONVERT(VARCHAR, COUNT(*)) FROM Cliente) [TotalClientes],
        (SELECT CONVERT(VARCHAR, COUNT(*)) FROM Prestamo WHERE Estado = 'Pendiente')[PrestamosPendientes],
        (SELECT CONVERT(VARCHAR, COUNT(*)) FROM Prestamo WHERE Estado = 'Cancelado')[PrestamosCancelados],
		(SELECT CONVERT(VARCHAR, COUNT(*)) FROM SolicitudPrestamo WHERE Estado = 'Pendiente')[SolicitudesPendientes],
        (SELECT CONVERT(VARCHAR, ISNULL(SUM(ValorInteres), 0)) FROM Prestamo WHERE Estado = 'Cancelado')[InteresAcumulado]
END
GO

--- PROCEDIMIENTOS DE AUDITORIA ---
CREATE PROCEDURE [dbo].[sp_insertarAuditoria]
    @Usuario NVARCHAR(256),
    @Accion NVARCHAR(256),
    @Fecha DATETIME,
    @Detalles NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO Auditoria (Usuario, Accion, Fecha, Detalles)
    VALUES (@Usuario, @Accion, @Fecha, @Detalles)
END
GO