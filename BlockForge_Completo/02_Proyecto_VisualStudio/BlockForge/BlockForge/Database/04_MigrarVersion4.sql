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
