-- Versión 3: cada usuario puede tener una skin (PNG de 64×64 o 64×32 con el formato de Minecraft).
ALTER TABLE Usuario ADD
    SkinPng     VARBINARY(MAX) NULL,
    SkinDelgada BIT NOT NULL CONSTRAINT DF_Usuario_SkinDelgada DEFAULT 0,
    SkinOrigen  VARCHAR(40) NULL;
GO

INSERT INTO VersionEsquema (Version) VALUES (3);
GO
