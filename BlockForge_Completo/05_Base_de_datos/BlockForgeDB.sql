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
