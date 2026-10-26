-- Versión 6: cuentas de jugador para Ruby, Madai y Danilo (cada una puede cambiar después su nombre
-- y su contraseña desde "Cambiar nombre o contraseña" en el menú principal).
DECLARE @sal VARBINARY(16);

IF NOT EXISTS (SELECT 1 FROM Usuario WHERE NombreUsuario = 'Ruby')
BEGIN
    SET @sal = CRYPT_GEN_RANDOM(16);
    INSERT INTO Usuario (NombreUsuario, ContrasenaHash, Sal, Rol)
    VALUES ('Ruby', HASHBYTES('SHA2_256', @sal + CAST('Ruby2026' AS VARBINARY(30))), @sal, 'Jugador');
END

IF NOT EXISTS (SELECT 1 FROM Usuario WHERE NombreUsuario = 'Madai')
BEGIN
    SET @sal = CRYPT_GEN_RANDOM(16);
    INSERT INTO Usuario (NombreUsuario, ContrasenaHash, Sal, Rol)
    VALUES ('Madai', HASHBYTES('SHA2_256', @sal + CAST('Madai2026' AS VARBINARY(30))), @sal, 'Jugador');
END

IF NOT EXISTS (SELECT 1 FROM Usuario WHERE NombreUsuario = 'Danilo')
BEGIN
    SET @sal = CRYPT_GEN_RANDOM(16);
    INSERT INTO Usuario (NombreUsuario, ContrasenaHash, Sal, Rol)
    VALUES ('Danilo', HASHBYTES('SHA2_256', @sal + CAST('Danilo2026' AS VARBINARY(30))), @sal, 'Jugador');
END

INSERT INTO VersionEsquema (Version) VALUES (6);
GO
