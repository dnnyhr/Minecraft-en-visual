-- Versión 5: hierro, diamante y espadas (nueva categoría Herramienta), más colores de lana y bloques
-- de Minecraft Classic (roca, piedra musgosa, obsidiana) para usar con sus texturas.
ALTER TABLE TipoBloque DROP CONSTRAINT CK_TipoBloque_Categoria;
ALTER TABLE TipoBloque DROP CONSTRAINT DF_TipoBloque_Categoria;
ALTER TABLE TipoBloque ALTER COLUMN Categoria VARCHAR(12) NOT NULL;
ALTER TABLE TipoBloque ADD CONSTRAINT DF_TipoBloque_Categoria DEFAULT 'Bloque' FOR Categoria;
ALTER TABLE TipoBloque ADD CONSTRAINT CK_TipoBloque_Categoria
    CHECK (Categoria IN ('Bloque', 'Planta', 'Liquido', 'Comida', 'Material', 'Herramienta'));
GO

INSERT INTO TipoBloque (Nombre, ColorHex, EsSolido, EsRecolectable, Categoria, Alimento, Luz)
SELECT v.Nombre, v.ColorHex, v.EsSolido, v.EsRecolectable, v.Categoria, v.Alimento, v.Luz
FROM (VALUES
    ('Roca',                '#7E7E7E', 1, 1, 'Bloque',      0, 0),
    ('Piedra musgosa',      '#5E7A5A', 1, 1, 'Bloque',      0, 0),
    ('Obsidiana',           '#1A1426', 1, 1, 'Bloque',      0, 0),
    ('Mineral de hierro',   '#B89A86', 1, 1, 'Bloque',      0, 0),
    ('Mineral de diamante', '#6ADCDC', 1, 1, 'Bloque',      0, 0),
    ('Bloque de hierro',    '#DCDCDC', 1, 1, 'Bloque',      0, 0),
    ('Bloque de diamante',  '#64E6E0', 1, 1, 'Bloque',      0, 0),
    ('Lana naranja',        '#E07A2A', 1, 1, 'Bloque',      0, 0),
    ('Lana verde',          '#4C9A2A', 1, 1, 'Bloque',      0, 0),
    ('Lana celeste',        '#5AB4E0', 1, 1, 'Bloque',      0, 0),
    ('Lana morada',         '#7A3AB4', 1, 1, 'Bloque',      0, 0),
    ('Lana rosa',           '#E88AAE', 1, 1, 'Bloque',      0, 0),
    ('Lana gris',           '#6A6A6E', 1, 1, 'Bloque',      0, 0),
    ('Lingote de hierro',   '#D8D8D8', 0, 1, 'Material',    0, 0),
    ('Diamante',            '#5AE6E0', 0, 1, 'Material',    0, 0),
    ('Espada de madera',    '#A8844E', 0, 1, 'Herramienta', 0, 0),
    ('Espada de piedra',    '#8A8A8A', 0, 1, 'Herramienta', 0, 0),
    ('Espada de hierro',    '#E0E0E0', 0, 1, 'Herramienta', 0, 0),
    ('Espada de diamante',  '#5AE6E0', 0, 1, 'Herramienta', 0, 0)
) v (Nombre, ColorHex, EsSolido, EsRecolectable, Categoria, Alimento, Luz)
WHERE NOT EXISTS (SELECT 1 FROM TipoBloque t WHERE t.Nombre = v.Nombre);

INSERT INTO VersionEsquema (Version) VALUES (5);
GO
