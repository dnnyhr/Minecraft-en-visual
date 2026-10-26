-- Migra una base BlockForgeDB de la versión 1 (mundos 2D) a la versión 2 (mundos 3D).
-- La aplicación lo ejecuta sola, dentro de una transacción, si no encuentra la tabla VersionEsquema.
-- Los mundos 2D no se pueden convertir a 3D: se regeneran a partir de su semilla al abrirlos
-- (se conservan el nombre, el dueño, el inventario y el historial de partidas).

ALTER TABLE Mundo DROP CONSTRAINT CK_Mundo_Ancho, CK_Mundo_Alto;
ALTER TABLE Mundo ADD Largo INT NULL, Bloques VARBINARY(MAX) NULL,
    JugadorX REAL NULL, JugadorY REAL NULL, JugadorZ REAL NULL,
    JugadorGiro REAL NULL, JugadorInclinacion REAL NULL;
GO

UPDATE Mundo SET
    Ancho = CASE WHEN Ancho < 32 THEN 32 ELSE Ancho END,
    Largo = CASE WHEN Ancho < 32 THEN 32 ELSE Ancho END,
    Alto  = CASE WHEN Alto  < 32 THEN 32 ELSE Alto END;
ALTER TABLE Mundo ALTER COLUMN Largo INT NOT NULL;
GO

ALTER TABLE Mundo ADD
    CONSTRAINT CK_Mundo_Ancho CHECK (Ancho BETWEEN 32 AND 256),
    CONSTRAINT CK_Mundo_Largo CHECK (Largo BETWEEN 32 AND 256),
    CONSTRAINT CK_Mundo_Alto  CHECK (Alto  BETWEEN 32 AND 128);
DROP TABLE Bloque;
GO

-- Configuración: el tamaño de celda y la cuadrícula ya no aplican en 3D.
ALTER TABLE Configuracion DROP CONSTRAINT CK_Config_Celda, CK_Config_Ancho, CK_Config_Alto;
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql = @sql + N'ALTER TABLE Configuracion DROP CONSTRAINT ' + QUOTENAME(dc.name) + N'; '
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID('Configuracion')
  AND c.name IN ('TamanoCelda', 'MostrarCuadricula', 'AnchoPredeterminado', 'AltoPredeterminado');
EXEC sp_executesql @sql;
ALTER TABLE Configuracion DROP COLUMN TamanoCelda, MostrarCuadricula;
ALTER TABLE Configuracion ADD
    DistanciaVision     INT NOT NULL CONSTRAINT DF_Config_Distancia DEFAULT 128,
    CampoVision         INT NOT NULL CONSTRAINT DF_Config_Campo DEFAULT 70,
    Sensibilidad        INT NOT NULL CONSTRAINT DF_Config_Sensibilidad DEFAULT 5,
    LargoPredeterminado INT NOT NULL CONSTRAINT DF_Config_Largo DEFAULT 128;
GO

UPDATE Configuracion SET AnchoPredeterminado = 128, LargoPredeterminado = 128, AltoPredeterminado = 64;
ALTER TABLE Configuracion ADD
    CONSTRAINT DF_Config_Ancho DEFAULT 128 FOR AnchoPredeterminado,
    CONSTRAINT DF_Config_Alto DEFAULT 64 FOR AltoPredeterminado,
    CONSTRAINT CK_Config_Distancia CHECK (DistanciaVision BETWEEN 32 AND 256),
    CONSTRAINT CK_Config_Campo CHECK (CampoVision BETWEEN 50 AND 110),
    CONSTRAINT CK_Config_Sensibilidad CHECK (Sensibilidad BETWEEN 1 AND 10),
    CONSTRAINT CK_Config_Ancho CHECK (AnchoPredeterminado BETWEEN 32 AND 256),
    CONSTRAINT CK_Config_Largo CHECK (LargoPredeterminado BETWEEN 32 AND 256),
    CONSTRAINT CK_Config_Alto CHECK (AltoPredeterminado BETWEEN 32 AND 128);

-- Bloques nuevos del terreno 3D.
INSERT INTO TipoBloque (Nombre, ColorHex, EsSolido, EsRecolectable)
SELECT v.Nombre, v.ColorHex, 1, 1
FROM (VALUES ('Arena', '#DBCB8E'), ('Tronco', '#6B4F2A'), ('Hojas', '#3E8E2E'), ('Grava', '#837E7A')) v (Nombre, ColorHex)
WHERE NOT EXISTS (SELECT 1 FROM TipoBloque t WHERE t.Nombre = v.Nombre);

CREATE TABLE VersionEsquema (
    Version        INT       NOT NULL CONSTRAINT PK_VersionEsquema PRIMARY KEY,
    FechaAplicada  DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
INSERT INTO VersionEsquema (Version) VALUES (2);
GO
