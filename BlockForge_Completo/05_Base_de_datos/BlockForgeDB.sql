CREATE DATABASE BlockForgeDB;
GO
USE BlockForgeDB;
GO

-- Versión del esquema (la aplicación la usa para saber si debe migrar una base anterior).
CREATE TABLE VersionEsquema (
    Version        INT       NOT NULL CONSTRAINT PK_VersionEsquema PRIMARY KEY,
    FechaAplicada  DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE Usuario (
    IdUsuario      INT IDENTITY(1,1) CONSTRAINT PK_Usuario PRIMARY KEY,
    NombreUsuario  VARCHAR(30)   NOT NULL CONSTRAINT UQ_Usuario_Nombre UNIQUE,
    ContrasenaHash VARBINARY(32) NOT NULL,
    Sal            VARBINARY(16) NOT NULL,
    Rol            VARCHAR(15)   NOT NULL
        CONSTRAINT CK_Usuario_Rol CHECK (Rol IN ('Jugador', 'Administrador')),
    FechaRegistro  DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    Activo         BIT           NOT NULL DEFAULT 1,
    SkinPng        VARBINARY(MAX) NULL,
    SkinDelgada    BIT           NOT NULL CONSTRAINT DF_Usuario_SkinDelgada DEFAULT 0,
    SkinOrigen     VARCHAR(40)   NULL,
    CONSTRAINT CK_Usuario_Longitud CHECK (LEN(NombreUsuario) BETWEEN 4 AND 30)
);

CREATE TABLE TipoBloque (
    IdTipoBloque   INT IDENTITY(1,1) CONSTRAINT PK_TipoBloque PRIMARY KEY,
    Nombre         VARCHAR(20) NOT NULL CONSTRAINT UQ_TipoBloque_Nombre UNIQUE,
    ColorHex       CHAR(7)     NOT NULL
        CONSTRAINT CK_TipoBloque_Color CHECK (ColorHex LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'),
    EsSolido       BIT NOT NULL DEFAULT 1,
    EsRecolectable BIT NOT NULL DEFAULT 1
);

-- Los bloques del mundo se guardan como un arreglo de bytes comprimido con GZip
-- (1 byte por celda, orden Y-Z-X), igual que los mapas de ClassiCube.
CREATE TABLE Mundo (
    IdMundo            INT IDENTITY(1,1) CONSTRAINT PK_Mundo PRIMARY KEY,
    IdUsuario          INT NOT NULL CONSTRAINT FK_Mundo_Usuario REFERENCES Usuario (IdUsuario),
    Nombre             VARCHAR(40) NOT NULL,
    Ancho              INT NOT NULL CONSTRAINT CK_Mundo_Ancho CHECK (Ancho BETWEEN 32 AND 256),
    Largo              INT NOT NULL CONSTRAINT CK_Mundo_Largo CHECK (Largo BETWEEN 32 AND 256),
    Alto               INT NOT NULL CONSTRAINT CK_Mundo_Alto  CHECK (Alto  BETWEEN 32 AND 128),
    Semilla            INT NOT NULL,
    ConLagos           BIT NOT NULL DEFAULT 1,
    FechaCreacion      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Bloques            VARBINARY(MAX) NULL,
    JugadorX           REAL NULL,
    JugadorY           REAL NULL,
    JugadorZ           REAL NULL,
    JugadorGiro        REAL NULL,
    JugadorInclinacion REAL NULL,
    CONSTRAINT UQ_Mundo_Usuario_Nombre UNIQUE (IdUsuario, Nombre),
    CONSTRAINT CK_Mundo_Nombre CHECK (LEN(Nombre) BETWEEN 3 AND 40)
);

CREATE TABLE ItemInventario (
    IdItem         INT IDENTITY(1,1) CONSTRAINT PK_ItemInventario PRIMARY KEY,
    IdMundo        INT NOT NULL
        CONSTRAINT FK_Item_Mundo REFERENCES Mundo (IdMundo) ON DELETE CASCADE,
    IdTipoBloque   INT NOT NULL
        CONSTRAINT FK_Item_TipoBloque REFERENCES TipoBloque (IdTipoBloque),
    Cantidad       INT NOT NULL DEFAULT 0
        CONSTRAINT CK_Item_Cantidad CHECK (Cantidad BETWEEN 0 AND 999),
    CONSTRAINT UQ_Item_Mundo_Tipo UNIQUE (IdMundo, IdTipoBloque)
);

CREATE TABLE Partida (
    IdPartida      INT IDENTITY(1,1) CONSTRAINT PK_Partida PRIMARY KEY,
    IdMundo        INT NOT NULL
        CONSTRAINT FK_Partida_Mundo REFERENCES Mundo (IdMundo) ON DELETE CASCADE,
    FechaGuardado  DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TotalBloques   INT NOT NULL CONSTRAINT CK_Partida_Total CHECK (TotalBloques >= 0)
);

CREATE TABLE Configuracion (
    IdConfiguracion     INT IDENTITY(1,1) CONSTRAINT PK_Configuracion PRIMARY KEY,
    IdUsuarioModifica   INT NULL
        CONSTRAINT FK_Config_Usuario REFERENCES Usuario (IdUsuario) ON DELETE SET NULL,
    DistanciaVision     INT NOT NULL CONSTRAINT DF_Config_Distancia DEFAULT 128
        CONSTRAINT CK_Config_Distancia CHECK (DistanciaVision BETWEEN 32 AND 256),
    CampoVision         INT NOT NULL CONSTRAINT DF_Config_Campo DEFAULT 70
        CONSTRAINT CK_Config_Campo CHECK (CampoVision BETWEEN 50 AND 110),
    Sensibilidad        INT NOT NULL CONSTRAINT DF_Config_Sensibilidad DEFAULT 5
        CONSTRAINT CK_Config_Sensibilidad CHECK (Sensibilidad BETWEEN 1 AND 10),
    AnchoPredeterminado INT NOT NULL CONSTRAINT DF_Config_Ancho DEFAULT 128
        CONSTRAINT CK_Config_Ancho CHECK (AnchoPredeterminado BETWEEN 32 AND 256),
    LargoPredeterminado INT NOT NULL CONSTRAINT DF_Config_Largo DEFAULT 128
        CONSTRAINT CK_Config_Largo CHECK (LargoPredeterminado BETWEEN 32 AND 256),
    AltoPredeterminado  INT NOT NULL CONSTRAINT DF_Config_Alto DEFAULT 64
        CONSTRAINT CK_Config_Alto CHECK (AltoPredeterminado BETWEEN 32 AND 128),
    FechaModificacion   DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

-- Datos iniciales
INSERT INTO TipoBloque (Nombre, ColorHex, EsSolido, EsRecolectable) VALUES
    ('Pasto',  '#5FA046', 1, 1),
    ('Tierra', '#8B5A2B', 1, 1),
    ('Piedra', '#8A8A8A', 1, 1),
    ('Madera', '#A0703C', 1, 1),
    ('Agua',   '#3F76E4', 0, 0),
    ('Arena',  '#DBCB8E', 1, 1),
    ('Tronco', '#6B4F2A', 1, 1),
    ('Hojas',  '#3E8E2E', 1, 1),
    ('Grava',  '#837E7A', 1, 1);

INSERT INTO Configuracion DEFAULT VALUES;

-- Cuentas de prueba: hash = SHA-256(sal + contraseña)
DECLARE @sal VARBINARY(16) = CRYPT_GEN_RANDOM(16);
INSERT INTO Usuario (NombreUsuario, ContrasenaHash, Sal, Rol)
VALUES ('admin', HASHBYTES('SHA2_256', @sal + CAST('Admin2026' AS VARBINARY(30))), @sal, 'Administrador');

SET @sal = CRYPT_GEN_RANDOM(16);
INSERT INTO Usuario (NombreUsuario, ContrasenaHash, Sal, Rol)
VALUES ('jugador1', HASHBYTES('SHA2_256', @sal + CAST('Jugador2026' AS VARBINARY(30))), @sal, 'Jugador');

INSERT INTO VersionEsquema (Version) VALUES (3);
GO

-- ===== Versión 4 (supervivencia) =====
USE BlockForgeDB;
GO
-- Versión 4: modo supervivencia. Los tipos tienen categoría (bloque, planta, líquido, comida o material),
-- la luz que emiten y lo que alimentan; se agregan bloques de construcción, plantas, lava, minerales y comida.
-- Cada mundo guarda la hora del día y la vida y el hambre del jugador.
ALTER TABLE TipoBloque ADD
    Categoria VARCHAR(10) NOT NULL CONSTRAINT DF_TipoBloque_Categoria DEFAULT 'Bloque'
        CONSTRAINT CK_TipoBloque_Categoria CHECK (Categoria IN ('Bloque', 'Planta', 'Liquido', 'Comida', 'Material')),
    Alimento  INT NOT NULL CONSTRAINT DF_TipoBloque_Alimento DEFAULT 0
        CONSTRAINT CK_TipoBloque_Alimento CHECK (Alimento BETWEEN 0 AND 20),
    Luz       INT NOT NULL CONSTRAINT DF_TipoBloque_Luz DEFAULT 0
        CONSTRAINT CK_TipoBloque_Luz CHECK (Luz BETWEEN 0 AND 15);

ALTER TABLE Mundo ADD HoraDia REAL NULL, JugadorVida REAL NULL, JugadorHambre REAL NULL;
GO

UPDATE TipoBloque SET Categoria = 'Liquido' WHERE Nombre = 'Agua';

INSERT INTO TipoBloque (Nombre, ColorHex, EsSolido, EsRecolectable, Categoria, Alimento, Luz)
SELECT v.Nombre, v.ColorHex, v.EsSolido, v.EsRecolectable, v.Categoria, v.Alimento, v.Luz
FROM (VALUES
    ('Lava',                '#E8641E', 0, 0, 'Liquido',  0, 15),
    ('Mineral de carbón',   '#4A4A4A', 1, 1, 'Bloque',   0, 0),
    ('Arcilla',             '#A3A9B8', 1, 1, 'Bloque',   0, 0),
    ('Madera oscura',       '#5A3A1E', 1, 1, 'Bloque',   0, 0),
    ('Ladrillo',            '#9C4A36', 1, 1, 'Bloque',   0, 0),
    ('Ladrillos de piedra', '#7A7A7A', 1, 1, 'Bloque',   0, 0),
    ('Piedra lisa',         '#A8A8A8', 1, 1, 'Bloque',   0, 0),
    ('Terracota',           '#B0603E', 1, 1, 'Bloque',   0, 0),
    ('Vidrio',              '#C8E6F0', 1, 1, 'Bloque',   0, 0),
    ('Lana blanca',         '#EAEAEA', 1, 1, 'Bloque',   0, 0),
    ('Lana roja',           '#A12722', 1, 1, 'Bloque',   0, 0),
    ('Lana amarilla',       '#E5C62E', 1, 1, 'Bloque',   0, 0),
    ('Lana azul',           '#35399D', 1, 1, 'Bloque',   0, 0),
    ('Lana negra',          '#1C1C20', 1, 1, 'Bloque',   0, 0),
    ('Estantería',          '#8E6B3E', 1, 1, 'Bloque',   0, 0),
    ('Lámpara',             '#F5D27A', 1, 1, 'Bloque',   0, 15),
    ('Antorcha',            '#FFC94A', 0, 1, 'Planta',   0, 14),
    ('Rosa',                '#C8202A', 0, 1, 'Planta',   0, 0),
    ('Diente de león',      '#F2D32A', 0, 1, 'Planta',   0, 0),
    ('Aciano',              '#4064D8', 0, 1, 'Planta',   0, 0),
    ('Hierba alta',         '#5C9A3C', 0, 1, 'Planta',   0, 0),
    ('Brote',               '#4E8A2E', 0, 1, 'Planta',   0, 0),
    ('Carbón',              '#2A2A2A', 0, 1, 'Material', 0, 0),
    ('Manzana',             '#D8322E', 0, 1, 'Comida',   4, 0),
    ('Chuleta cruda',       '#E8908A', 0, 1, 'Comida',   3, 0),
    ('Chuleta cocida',      '#B8743E', 0, 1, 'Comida',   8, 0),
    ('Filete crudo',        '#C4483E', 0, 1, 'Comida',   3, 0),
    ('Filete cocido',       '#7A4A2A', 0, 1, 'Comida',   8, 0),
    ('Pollo crudo',         '#F0C8B4', 0, 1, 'Comida',   2, 0),
    ('Pollo cocido',        '#D09A5A', 0, 1, 'Comida',   6, 0),
    ('Carne podrida',       '#7A8A4A', 0, 1, 'Comida',   2, 0)
) v (Nombre, ColorHex, EsSolido, EsRecolectable, Categoria, Alimento, Luz)
WHERE NOT EXISTS (SELECT 1 FROM TipoBloque t WHERE t.Nombre = v.Nombre);

INSERT INTO VersionEsquema (Version) VALUES (4);
GO
