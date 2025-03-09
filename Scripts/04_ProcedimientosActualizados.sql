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
    DECLARE @Dominio NVARCHAR(100)

    -- Extraer el dominio del correo
    SET @Dominio = RIGHT(@Correo, CHARINDEX('@', REVERSE(@Correo)) - 1)

    -- Verificar si el dominio es válido
    IF @Dominio NOT IN ('espe.edu.ec', 'gmail.com', 'hotmail.com')
    BEGIN
        RAISERROR ('El dominio del correo no está permitido.', 16, 1)
        RETURN
    END

    -- Insertar el usuario si el dominio es válido
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

    -- Validar que el Nombre solo contenga letras
    IF @Nombre LIKE '%[^A-Za-z ]%'
    BEGIN
        SET @msgError = 'El nombre solo puede contener letras y espacios.'
        RETURN
    END

    -- Validar que el Símbolo tenga máximo 2 caracteres
    IF LEN(@Simbolo) > 2
    BEGIN
        SET @msgError = 'El símbolo debe tener un máximo de 2 caracteres.'
        RETURN
    END

    -- Verificar si la moneda ya existe (sensible a mayúsculas y minúsculas)
    IF NOT EXISTS (SELECT 1 FROM Moneda WHERE Nombre = @Nombre COLLATE SQL_Latin1_General_CP1_CS_AS)
    BEGIN
        INSERT INTO Moneda (Nombre, Simbolo) 
        VALUES (@Nombre, @Simbolo)
    END
    ELSE
    BEGIN
        SET @msgError = 'La moneda ya existe.'
    END
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

    -- Validar que el Nombre solo contenga letras y espacios
    IF @Nombre LIKE '%[^A-Za-z ]%'
    BEGIN
        SET @msgError = 'El nombre solo puede contener letras y espacios.'
        RETURN
    END

    -- Validar que el Símbolo tenga máximo 2 caracteres
    IF LEN(@Simbolo) > 2
    BEGIN
        SET @msgError = 'El símbolo debe tener un máximo de 2 caracteres.'
        RETURN
    END

    -- Verificar si ya existe otra moneda con el mismo nombre
    IF EXISTS (SELECT 1 FROM Moneda WHERE Nombre = @Nombre COLLATE SQL_Latin1_General_CP1_CS_AS AND IdMoneda != @IdMoneda)
    BEGIN
        SET @msgError = 'La moneda ya existe.'
        RETURN
    END

    -- Actualizar la moneda
    UPDATE Moneda 
    SET Nombre = @Nombre, Simbolo = @Simbolo 
    WHERE IdMoneda = @IdMoneda
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
        -- Validar que el Nombre solo contenga letras y espacios
        IF @Nombre LIKE '%[^A-Za-z ]%'
        BEGIN
            SET @msgError = 'El nombre solo puede contener letras y espacios.'
            RETURN
        END

        -- Validar que el Apellido solo contenga letras y espacios
        IF @Apellido LIKE '%[^A-Za-z ]%'
        BEGIN
            SET @msgError = 'El apellido solo puede contener letras y espacios.'
            RETURN
        END

        -- Validar que el Correo tenga un dominio permitido
        DECLARE @Dominio NVARCHAR(50)
        SET @Dominio = RIGHT(@Correo, CHARINDEX('@', REVERSE(@Correo)) - 1)

        IF @Dominio NOT IN ('epe.edu.ec', 'gmail.com', 'hotmail.com')
        BEGIN
            SET @msgError = 'El dominio del correo no está permitido.'
            RETURN
        END

        -- Validar que el Teléfono tenga 10 dígitos y comience con '09'
        IF LEN(@Telefono) != 10 OR @Telefono NOT LIKE '09%'
        BEGIN
            SET @msgError = 'El teléfono debe comenzar con 09 y contener exactamente 10 dígitos.'
            RETURN
        END

        -- Validar que la cédula tenga 10 dígitos y sean solo números
        IF LEN(@NroDocumento) != 10 OR @NroDocumento LIKE '%[^0-9]%'
        BEGIN
            SET @msgError = 'La cédula debe contener exactamente 10 dígitos numéricos.'
            RETURN
        END

        -- Obtener el código de provincia
        DECLARE @provincia INT
        SET @provincia = CAST(SUBSTRING(@NroDocumento, 1, 2) AS INT)
        IF @provincia < 1 OR (@provincia > 24 AND @provincia != 30)
        BEGIN
            SET @msgError = 'El código de provincia no es válido.'
            RETURN
        END

        -- Validación del dígito verificador de la cédula ecuatoriana
        DECLARE @suma INT = 0
        DECLARE @coeficientes TABLE (id INT IDENTITY(1,1), coef INT)
        INSERT INTO @coeficientes (coef) VALUES (2), (1), (2), (1), (2), (1), (2), (1), (2)

        DECLARE @i INT = 1
        WHILE @i <= 9
        BEGIN
            DECLARE @coef INT
            SELECT @coef = coef FROM @coeficientes WHERE id = @i

            DECLARE @digito INT
            SET @digito = CAST(SUBSTRING(@NroDocumento, @i, 1) AS INT) * @coef
            IF @digito > 9 SET @digito = @digito - 9
            SET @suma = @suma + @digito
            SET @i = @i + 1
        END

        DECLARE @digitoVerificadorCalculado INT
        SET @digitoVerificadorCalculado = (10 - (@suma % 10)) % 10
        DECLARE @digitoVerificadorCedula INT
        SET @digitoVerificadorCedula = CAST(SUBSTRING(@NroDocumento, 10, 1) AS INT)

        IF @digitoVerificadorCalculado != @digitoVerificadorCedula
        BEGIN
            SET @msgError = 'La cédula no es válida.'
            RETURN
        END

        -- Verificar si el cliente ya existe
        IF NOT EXISTS (SELECT 1 FROM Cliente WHERE NroDocumento = @NroDocumento) 
        BEGIN
            INSERT INTO Cliente (NroDocumento, Nombre, Apellido, Correo, Telefono, FechaCreacion)
            VALUES (@NroDocumento, @Nombre, @Apellido, @Correo, @Telefono, GETDATE())
            SET @IdCliente = SCOPE_IDENTITY();
        END 
        ELSE
        BEGIN
            SET @msgError = 'El cliente ya existe.'
        END
    END TRY
    BEGIN CATCH
        SET @msgError = ERROR_MESSAGE();
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
    SET NOCOUNT ON;
    SET @msgError = '';

    BEGIN TRY
        -- Validar que el Nombre solo contenga letras y espacios
        IF @Nombre LIKE '%[^A-Za-z ]%'
        BEGIN
            SET @msgError = 'El nombre solo puede contener letras y espacios.'
            RETURN
        END

        -- Validar que el Apellido solo contenga letras y espacios
        IF @Apellido LIKE '%[^A-Za-z ]%'
        BEGIN
            SET @msgError = 'El apellido solo puede contener letras y espacios.'
            RETURN
        END

        -- Validar que el Correo tenga un dominio permitido
        DECLARE @Dominio NVARCHAR(50)
        SET @Dominio = RIGHT(@Correo, CHARINDEX('@', REVERSE(@Correo)) - 1)

        IF @Dominio NOT IN ('epe.edu.ec', 'gmail.com', 'hotmail.com')
        BEGIN
            SET @msgError = 'El dominio del correo no está permitido. Use epe.edu.ec, gmail.com o hotmail.com.'
            RETURN
        END

        -- Validar que el Teléfono tenga 10 dígitos y comience con '09'
        IF LEN(@Telefono) != 10 OR @Telefono NOT LIKE '09%'
        BEGIN
            SET @msgError = 'El teléfono debe comenzar con 09 y contener exactamente 10 dígitos.'
            RETURN
        END

        -- Validar que la cédula tenga 10 dígitos y sean solo números
        IF LEN(@NroDocumento) != 10 OR @NroDocumento LIKE '%[^0-9]%'
        BEGIN
            SET @msgError = 'La cédula debe contener exactamente 10 dígitos numéricos.'
            RETURN
        END

        -- Obtener el código de provincia
        DECLARE @provincia INT
        SET @provincia = CAST(SUBSTRING(@NroDocumento, 1, 2) AS INT)
        IF @provincia < 1 OR (@provincia > 24 AND @provincia != 30)
        BEGIN
            SET @msgError = 'El código de provincia no es válido.'
            RETURN
        END

        -- Validación del dígito verificador de la cédula ecuatoriana
        DECLARE @suma INT = 0
        DECLARE @coeficientes TABLE (id INT IDENTITY(1,1), coef INT)
        INSERT INTO @coeficientes (coef) VALUES (2), (1), (2), (1), (2), (1), (2), (1), (2)

        DECLARE @i INT = 1
        WHILE @i <= 9
        BEGIN
            DECLARE @coef INT
            SELECT @coef = coef FROM @coeficientes WHERE id = @i

            DECLARE @digito INT
            SET @digito = CAST(SUBSTRING(@NroDocumento, @i, 1) AS INT) * @coef
            IF @digito > 9 SET @digito = @digito - 9
            SET @suma = @suma + @digito
            SET @i = @i + 1
        END

        DECLARE @digitoVerificadorCalculado INT
        SET @digitoVerificadorCalculado = (10 - (@suma % 10)) % 10
        DECLARE @digitoVerificadorCedula INT
        SET @digitoVerificadorCedula = CAST(SUBSTRING(@NroDocumento, 10, 1) AS INT)

        IF @digitoVerificadorCalculado != @digitoVerificadorCedula
        BEGIN
            SET @msgError = 'La cédula no es válida.'
            RETURN
        END

        -- Verificar si ya existe otro cliente con la misma cédula
        IF EXISTS (SELECT 1 FROM Cliente WHERE NroDocumento = @NroDocumento AND IdCliente != @IdCliente)
        BEGIN
            SET @msgError = 'El cliente ya existe.'
            RETURN
        END

        -- Actualizar el cliente
        UPDATE Cliente 
        SET 
            NroDocumento = @NroDocumento,
            Nombre = @Nombre,
            Apellido = @Apellido,
            Correo = @Correo,
            Telefono = @Telefono 
        WHERE IdCliente = @IdCliente
    END TRY
    BEGIN CATCH
        SET @msgError = ERROR_MESSAGE();
    END CATCH
END
GO


CREATE PROCEDURE [dbo].[sp_eliminarCliente]
    @IdCliente INT,
    @msgError VARCHAR(100) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @msgError = '';

    BEGIN TRY
        -- Verificar si el cliente tiene préstamos asociados
        IF NOT EXISTS(SELECT IdPrestamo FROM Prestamo WHERE IdCliente = @IdCliente)
        BEGIN
            -- Obtener el correo del cliente
            DECLARE @Correo VARCHAR(50);
            SELECT @Correo = Correo FROM Cliente WHERE IdCliente = @IdCliente;

            -- Eliminar la cuenta asociada del cliente
            DELETE FROM Cuenta WHERE IdCliente = @IdCliente;

            -- Eliminar el cliente
            DELETE FROM Cliente WHERE IdCliente = @IdCliente;

            -- Eliminar el usuario asociado utilizando el correo electrónico
            DELETE FROM Usuario WHERE Correo = @Correo;
        END
        ELSE
        BEGIN
            SET @msgError = 'El cliente tiene préstamos asociados';
        END
    END TRY
    BEGIN CATCH
        SET @msgError = ERROR_MESSAGE();
    END CATCH
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
    SET DATEFORMAT dmy;
    SET @msgError = '';

    BEGIN TRY
        -- Validaciones
        IF LEN(@NroDocumento) <> 10
        BEGIN
            SET @msgError = 'El número de documento debe tener 10 caracteres.';
            RETURN;
        END

        IF @Nombre LIKE '%[^A-Za-zÁÉÍÓÚáéíóúÑñ ]%' OR @Apellido LIKE '%[^A-Za-zÁÉÍÓÚáéíóúÑñ ]%'
		BEGIN
			SET @msgError = 'El nombre y apellido solo deben contener letras y espacios.';
			RETURN;
		END

        IF @Correo NOT LIKE '%@espe.edu.ec' 
           AND @Correo NOT LIKE '%@gmail.com' 
           AND @Correo NOT LIKE '%@hotmail.com'
        BEGIN
            SET @msgError = 'El correo debe pertenecer a los dominios espe.edu.ec, gmail.com o hotmail.com.';
            RETURN;
        END

        IF @Telefono NOT LIKE '09%' OR LEN(@Telefono) <> 10
        BEGIN
            SET @msgError = 'El teléfono debe comenzar con 09 y tener exactamente 10 dígitos.';
            RETURN;
        END

        DECLARE @FecInicio DATE = CONVERT(DATE, @FechaInicio);
        IF @FecInicio <= GETDATE()
        BEGIN
            SET @msgError = 'La fecha de inicio debe ser una fecha futura.';
            RETURN;
        END

        IF @InteresPorcentaje NOT LIKE '[0-9]' AND @InteresPorcentaje NOT LIKE '[0-9][0-9]'
		BEGIN
			SET @msgError = 'El interés debe ser un número de uno o dos dígitos.';
			RETURN;
		END

        -- Conversión de valores numéricos
        DECLARE @MontPrestamo DECIMAL(10,2) = CONVERT(DECIMAL(10,2), @MontoPrestamo);
        DECLARE @IntPorcentaje DECIMAL(10,2) = CONVERT(DECIMAL(10,2), @InteresPorcentaje);
        DECLARE @VlrPorCuota DECIMAL(10,2) = CONVERT(DECIMAL(10,2), @ValorPorCuota);
        DECLARE @VlrInteres DECIMAL(10,2) = CONVERT(DECIMAL(10,2), @ValorInteres);
        DECLARE @VlrTotal DECIMAL(10,2) = CONVERT(DECIMAL(10,2), @ValorTotal);
        
        CREATE TABLE #TempIdentity(Id INT, Nombre VARCHAR(10));

        BEGIN TRANSACTION;

        -- Verificación de Cliente
        IF (@IdCliente = 0)
        BEGIN
            INSERT INTO Cliente(NroDocumento, Nombre, Apellido, Correo, Telefono)
            OUTPUT INSERTED.IdCliente, 'Cliente' INTO #TempIdentity(Id, Nombre)
            VALUES (@NroDocumento, @Nombre, @Apellido, @Correo, @Telefono);

            SET @IdCliente = (SELECT Id FROM #TempIdentity WHERE Nombre = 'Cliente');
        END
        ELSE
        BEGIN
            IF EXISTS (SELECT * FROM Prestamo WHERE IdCliente = @IdCliente AND Estado = 'Pendiente')
            BEGIN
                SET @msgError = 'El cliente tiene un préstamo pendiente, debe cancelar el anterior.';
                ROLLBACK TRANSACTION;
                RETURN;
            END
        END

        -- Inserción en Prestamo
        IF (@msgError = '')
        BEGIN
            INSERT INTO Prestamo(IdCliente, IdMoneda, FechaInicioPago, MontoPrestamo, InteresPorcentaje, NroCuotas, FormaDePago, ValorPorCuota, ValorInteres, ValorTotal, Estado)
            OUTPUT INSERTED.IdPrestamo, 'Prestamo' INTO #TempIdentity(Id, Nombre)
            VALUES (@IdCliente, @IdMoneda, @FecInicio, @MontPrestamo, @IntPorcentaje, @NroCuotas, @FormaDePago, @VlrPorCuota, @VlrInteres, @VlrTotal, 'Pendiente');

            -- Generación de detalles del préstamo
            ;WITH Detalle(IdPrestamo, FechaPago, NroCuota, MontoCuota, Estado) AS
            (
                SELECT (SELECT Id FROM #TempIdentity WHERE Nombre = 'Prestamo'), @FecInicio, 0, @VlrPorCuota, 'Pendiente'
                UNION ALL
                SELECT IdPrestamo,
                    CASE @FormaDePago 
                        WHEN 'Diario' THEN DATEADD(DAY, 1, FechaPago)
                        WHEN 'Semanal' THEN DATEADD(WEEK, 1, FechaPago)
                        WHEN 'Quincenal' THEN DATEADD(DAY, 15, FechaPago)
                        WHEN 'Mensual' THEN DATEADD(MONTH, 1, FechaPago)
                    END,
                    NroCuota + 1, MontoCuota, Estado
                FROM Detalle
                WHERE NroCuota < @NroCuotas
            )
            SELECT IdPrestamo, FechaPago, NroCuota, MontoCuota, Estado INTO #TempDetalle FROM Detalle WHERE NroCuota > 0;

            INSERT INTO PrestamoDetalle(IdPrestamo, FechaPago, NroCuota, MontoCuota, Estado)
            SELECT IdPrestamo, FechaPago, NroCuota, MontoCuota, Estado FROM #TempDetalle;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @msgError = ERROR_MESSAGE();
    END CATCH
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
        DECLARE @CuotasPendientes INT;

        -- Obtener el IdCliente y el total a pagar
        SELECT @IdCliente = p.IdCliente,
               @TotalPagar = SUM(pd.MontoCuota)
        FROM Prestamo p
        INNER JOIN PrestamoDetalle pd ON p.IdPrestamo = pd.IdPrestamo
        INNER JOIN dbo.SplitString(@NroCuotasPagadas, ',') ss ON ss.valor = pd.NroCuota
        WHERE p.IdPrestamo = @IdPrestamo
        GROUP BY p.IdCliente;

        -- Verificar si hay cuotas para pagar
        IF @TotalPagar IS NULL
        BEGIN
            SET @msgError = 'No se encontraron cuotas válidas para pagar';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Verificar el saldo de la cuenta
        DECLARE @SaldoCuenta DECIMAL(18, 2);
        DECLARE @Tarjeta NVARCHAR(64);
        DECLARE @TarjetaDesencriptada NVARCHAR(16);

        -- Obtener el saldo y la tarjeta encriptada
        SELECT @SaldoCuenta = c.Monto, 
               @Tarjeta = c.Tarjeta
        FROM Cuenta c
        WHERE c.IdCliente = @IdCliente;

        -- Verificar si se encontró la cuenta
        IF @SaldoCuenta IS NULL
        BEGIN
            SET @msgError = 'No se encontró la cuenta del cliente';
            ROLLBACK TRANSACTION;
            RETURN;
        END

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
            SET @msgError = 'Fondos insuficientes';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Actualizar el saldo de la cuenta
        UPDATE Cuenta
        SET Monto = Monto - @TotalPagar
        WHERE IdCliente = @IdCliente;

        -- Actualizar el estado de las cuotas
        UPDATE pd
        SET pd.Estado = 'Cancelado', 
            FechaPagado = GETDATE()
        FROM PrestamoDetalle pd
        INNER JOIN dbo.SplitString(@NroCuotasPagadas, ',') ss ON ss.valor = pd.NroCuota
        WHERE pd.IdPrestamo = @IdPrestamo;

        -- Verificar cuotas pendientes después de la actualización
        SELECT @CuotasPendientes = COUNT(*)
        FROM PrestamoDetalle
        WHERE IdPrestamo = @IdPrestamo 
        AND Estado = 'Pendiente';

        -- Actualizar el estado del préstamo si no quedan cuotas pendientes
        IF @CuotasPendientes = 0
        BEGIN
            UPDATE Prestamo
            SET Estado = 'Cancelado'
            WHERE IdPrestamo = @IdPrestamo;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SET @msgError = 'Error en sp_pagarCuotas: ' + ERROR_MESSAGE() + 
                       ' | IdPrestamo: ' + CAST(@IdPrestamo AS VARCHAR) + 
                       ' | NroCuotasPagadas: ' + @NroCuotasPagadas;
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
    @Clave NVARCHAR(100),
    @msgError NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @msgError = '';

    -- Validar longitud mínima de 12 caracteres
    IF LEN(@Clave) < 12
    BEGIN
        SET @msgError = 'La contraseña debe tener al menos 12 caracteres.';
        RETURN;
    END

    -- Validar que contenga al menos una mayúscula, un número y un carácter especial
    IF @Clave NOT LIKE '%[A-Z]%' OR @Clave NOT LIKE '%[0-9]%' OR @Clave NOT LIKE '%[^A-Za-z0-9]%'
    BEGIN
        SET @msgError = 'La contraseña debe contener al menos una mayúscula, un número y un carácter especial.';
        RETURN;
    END

    -- Validar que no contenga caracteres prohibidos (' , = , OR)
    IF @Clave LIKE '%''%' OR @Clave LIKE '%=%' OR @Clave LIKE '% OR %'
    BEGIN
        SET @msgError = 'La contraseña no puede contener los caracteres: '' , = , OR.';
        RETURN;
    END

    -- Actualizar la contraseña si pasa todas las validaciones
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
    @Ocupacion NVARCHAR(100),
    @msgError NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @msgError = '';

    -- Validar que los valores sean positivos
    IF @Monto <= 0
    BEGIN
        SET @msgError = 'El monto debe ser un número positivo.';
        RETURN;
    END

    IF @Plazo <= 0
    BEGIN
        SET @msgError = 'El plazo debe ser un número positivo.';
        RETURN;
    END

    IF @Sueldo <= 0
    BEGIN
        SET @msgError = 'El sueldo debe ser un número positivo.';
        RETURN;
    END

    IF @NumeroHijos < 0
    BEGIN
        SET @msgError = 'El número de hijos no puede ser negativo.';
        RETURN;
    END

    -- Validar que la ocupación solo contenga letras y espacios
    IF @Ocupacion LIKE '%[^A-Za-zÁÉÍÓÚáéíóúÑñ ]%'
    BEGIN
        SET @msgError = 'La ocupación solo puede contener letras y espacios.';
        RETURN;
    END

    -- Validar que el usuario no tenga una solicitud en estado 'Pendiente'
    IF EXISTS (SELECT 1 FROM SolicitudPrestamo WHERE IdUsuario = @IdUsuario AND Estado = 'Pendiente')
    BEGIN
        SET @msgError = 'Ya tienes una solicitud de préstamo en estado Pendiente.';
        RETURN;
    END

    -- Insertar en la tabla si pasa todas las validaciones
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

Create procedure sp_obtenerPrestamos2(
@IdPrestamo int = 0,
@NroDocumento varchar(50) = ''
)as
begin
	select p.IdPrestamo,
	c.IdCliente,c.NroDocumento,c.Nombre,c.Apellido,c.Correo,c.Telefono,
	m.IdMoneda,m.Nombre[NombreMoneda],m.Simbolo,
	CONVERT(char(10),p.FechaInicioPago, 103) [FechaInicioPago],
	CONVERT(VARCHAR,p.MontoPrestamo)[MontoPrestamo],
	CONVERT(VARCHAR,p.InteresPorcentaje)[InteresPorcentaje],
	p.NroCuotas,
	p.FormaDePago,
	CONVERT(VARCHAR,p.ValorPorCuota)[ValorPorCuota],
	CONVERT(VARCHAR,p.ValorInteres)[ValorInteres],
	CONVERT(VARCHAR,p.ValorTotal)[ValorTotal],
	p.Estado,
	CONVERT(char(10),p.FechaCreacion, 103) [FechaCreacion],
	(
		select pd.IdPrestamoDetalle,CONVERT(char(10),pd.FechaPago, 103) [FechaPago],
		CONVERT(VARCHAR,pd.MontoCuota)[MontoCuota],
		pd.NroCuota,pd.Estado,isnull(CONVERT(varchar(10),pd.FechaPagado, 103),'')[FechaPagado]
		from PrestamoDetalle pd
		where pd.IdPrestamo = p.IdPrestamo
		FOR XML PATH('Detalle'), TYPE, ROOT('PrestamoDetalle')
	)
	from Prestamo p
	inner join Cliente c on c.IdCliente = p.IdCliente
	inner join Moneda m on m.IdMoneda = p.IdMoneda
	where p.IdPrestamo = iif(@IdPrestamo = 0,p.idprestamo,@IdPrestamo) and
	c.NroDocumento = iif(@NroDocumento = '',c.NroDocumento,@NroDocumento)
	FOR XML PATH('Prestamo'), ROOT('Prestamos'), TYPE;
end
GO