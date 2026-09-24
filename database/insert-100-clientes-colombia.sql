-- 100 clientes ficticios para pruebas. No representan personas ni empresas reales.
-- Identificaciones sintéticas DEMO-CO-001 a DEMO-CO-100, tipo Nit = 2.
-- Ejecutar sobre PruebaTecnicaDMC. Conserva clientes existentes y omite este lote si ya está insertado.
-- Ubicaciones tomadas del catálogo activo de Colombia (170), distribuidas entre departamentos.
USE [PruebaTecnicaDMC];
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Clientes TABLE (
        Numero int PRIMARY KEY,
        Identificacion varchar(30) NOT NULL UNIQUE,
        RazonSocial varchar(150) NOT NULL
    );

    INSERT INTO @Clientes (Numero, Identificacion, RazonSocial)
    VALUES
    (1, 'DEMO-CO-001', 'Comercializadora Andina Demo 001 SAS'),
    (2, 'DEMO-CO-002', 'Distribuidora del Norte Demo 002 SAS'),
    (3, 'DEMO-CO-003', 'Servicios del Pacifico Demo 003 SAS'),
    (4, 'DEMO-CO-004', 'Logistica Central Demo 004 SAS'),
    (5, 'DEMO-CO-005', 'Soluciones del Caribe Demo 005 SAS'),
    (6, 'DEMO-CO-006', 'Insumos del Oriente Demo 006 SAS'),
    (7, 'DEMO-CO-007', 'Tecnologia del Valle Demo 007 SAS'),
    (8, 'DEMO-CO-008', 'Transportes del Sur Demo 008 SAS'),
    (9, 'DEMO-CO-009', 'Alimentos de la Sabana Demo 009 SAS'),
    (10, 'DEMO-CO-010', 'Suministros del Llano Demo 010 SAS'),
    (11, 'DEMO-CO-011', 'Consultoria Integral Demo 011 SAS'),
    (12, 'DEMO-CO-012', 'Manufacturas del Centro Demo 012 SAS'),
    (13, 'DEMO-CO-013', 'Agroinsumos Nacionales Demo 013 SAS'),
    (14, 'DEMO-CO-014', 'Equipos Industriales Demo 014 SAS'),
    (15, 'DEMO-CO-015', 'Papeleria Empresarial Demo 015 SAS'),
    (16, 'DEMO-CO-016', 'Textiles Regionales Demo 016 SAS'),
    (17, 'DEMO-CO-017', 'Ferreteria Mayorista Demo 017 SAS'),
    (18, 'DEMO-CO-018', 'Servicios Ambientales Demo 018 SAS'),
    (19, 'DEMO-CO-019', 'Productos del Campo Demo 019 SAS'),
    (20, 'DEMO-CO-020', 'Comercio Digital Demo 020 SAS'),
    (21, 'DEMO-CO-021', 'Comercializadora Andina Demo 021 SAS'),
    (22, 'DEMO-CO-022', 'Distribuidora del Norte Demo 022 SAS'),
    (23, 'DEMO-CO-023', 'Servicios del Pacifico Demo 023 SAS'),
    (24, 'DEMO-CO-024', 'Logistica Central Demo 024 SAS'),
    (25, 'DEMO-CO-025', 'Soluciones del Caribe Demo 025 SAS'),
    (26, 'DEMO-CO-026', 'Insumos del Oriente Demo 026 SAS'),
    (27, 'DEMO-CO-027', 'Tecnologia del Valle Demo 027 SAS'),
    (28, 'DEMO-CO-028', 'Transportes del Sur Demo 028 SAS'),
    (29, 'DEMO-CO-029', 'Alimentos de la Sabana Demo 029 SAS'),
    (30, 'DEMO-CO-030', 'Suministros del Llano Demo 030 SAS'),
    (31, 'DEMO-CO-031', 'Consultoria Integral Demo 031 SAS'),
    (32, 'DEMO-CO-032', 'Manufacturas del Centro Demo 032 SAS'),
    (33, 'DEMO-CO-033', 'Agroinsumos Nacionales Demo 033 SAS'),
    (34, 'DEMO-CO-034', 'Equipos Industriales Demo 034 SAS'),
    (35, 'DEMO-CO-035', 'Papeleria Empresarial Demo 035 SAS'),
    (36, 'DEMO-CO-036', 'Textiles Regionales Demo 036 SAS'),
    (37, 'DEMO-CO-037', 'Ferreteria Mayorista Demo 037 SAS'),
    (38, 'DEMO-CO-038', 'Servicios Ambientales Demo 038 SAS'),
    (39, 'DEMO-CO-039', 'Productos del Campo Demo 039 SAS'),
    (40, 'DEMO-CO-040', 'Comercio Digital Demo 040 SAS'),
    (41, 'DEMO-CO-041', 'Comercializadora Andina Demo 041 SAS'),
    (42, 'DEMO-CO-042', 'Distribuidora del Norte Demo 042 SAS'),
    (43, 'DEMO-CO-043', 'Servicios del Pacifico Demo 043 SAS'),
    (44, 'DEMO-CO-044', 'Logistica Central Demo 044 SAS'),
    (45, 'DEMO-CO-045', 'Soluciones del Caribe Demo 045 SAS'),
    (46, 'DEMO-CO-046', 'Insumos del Oriente Demo 046 SAS'),
    (47, 'DEMO-CO-047', 'Tecnologia del Valle Demo 047 SAS'),
    (48, 'DEMO-CO-048', 'Transportes del Sur Demo 048 SAS'),
    (49, 'DEMO-CO-049', 'Alimentos de la Sabana Demo 049 SAS'),
    (50, 'DEMO-CO-050', 'Suministros del Llano Demo 050 SAS'),
    (51, 'DEMO-CO-051', 'Consultoria Integral Demo 051 SAS'),
    (52, 'DEMO-CO-052', 'Manufacturas del Centro Demo 052 SAS'),
    (53, 'DEMO-CO-053', 'Agroinsumos Nacionales Demo 053 SAS'),
    (54, 'DEMO-CO-054', 'Equipos Industriales Demo 054 SAS'),
    (55, 'DEMO-CO-055', 'Papeleria Empresarial Demo 055 SAS'),
    (56, 'DEMO-CO-056', 'Textiles Regionales Demo 056 SAS'),
    (57, 'DEMO-CO-057', 'Ferreteria Mayorista Demo 057 SAS'),
    (58, 'DEMO-CO-058', 'Servicios Ambientales Demo 058 SAS'),
    (59, 'DEMO-CO-059', 'Productos del Campo Demo 059 SAS'),
    (60, 'DEMO-CO-060', 'Comercio Digital Demo 060 SAS'),
    (61, 'DEMO-CO-061', 'Comercializadora Andina Demo 061 SAS'),
    (62, 'DEMO-CO-062', 'Distribuidora del Norte Demo 062 SAS'),
    (63, 'DEMO-CO-063', 'Servicios del Pacifico Demo 063 SAS'),
    (64, 'DEMO-CO-064', 'Logistica Central Demo 064 SAS'),
    (65, 'DEMO-CO-065', 'Soluciones del Caribe Demo 065 SAS'),
    (66, 'DEMO-CO-066', 'Insumos del Oriente Demo 066 SAS'),
    (67, 'DEMO-CO-067', 'Tecnologia del Valle Demo 067 SAS'),
    (68, 'DEMO-CO-068', 'Transportes del Sur Demo 068 SAS'),
    (69, 'DEMO-CO-069', 'Alimentos de la Sabana Demo 069 SAS'),
    (70, 'DEMO-CO-070', 'Suministros del Llano Demo 070 SAS'),
    (71, 'DEMO-CO-071', 'Consultoria Integral Demo 071 SAS'),
    (72, 'DEMO-CO-072', 'Manufacturas del Centro Demo 072 SAS'),
    (73, 'DEMO-CO-073', 'Agroinsumos Nacionales Demo 073 SAS'),
    (74, 'DEMO-CO-074', 'Equipos Industriales Demo 074 SAS'),
    (75, 'DEMO-CO-075', 'Papeleria Empresarial Demo 075 SAS'),
    (76, 'DEMO-CO-076', 'Textiles Regionales Demo 076 SAS'),
    (77, 'DEMO-CO-077', 'Ferreteria Mayorista Demo 077 SAS'),
    (78, 'DEMO-CO-078', 'Servicios Ambientales Demo 078 SAS'),
    (79, 'DEMO-CO-079', 'Productos del Campo Demo 079 SAS'),
    (80, 'DEMO-CO-080', 'Comercio Digital Demo 080 SAS'),
    (81, 'DEMO-CO-081', 'Comercializadora Andina Demo 081 SAS'),
    (82, 'DEMO-CO-082', 'Distribuidora del Norte Demo 082 SAS'),
    (83, 'DEMO-CO-083', 'Servicios del Pacifico Demo 083 SAS'),
    (84, 'DEMO-CO-084', 'Logistica Central Demo 084 SAS'),
    (85, 'DEMO-CO-085', 'Soluciones del Caribe Demo 085 SAS'),
    (86, 'DEMO-CO-086', 'Insumos del Oriente Demo 086 SAS'),
    (87, 'DEMO-CO-087', 'Tecnologia del Valle Demo 087 SAS'),
    (88, 'DEMO-CO-088', 'Transportes del Sur Demo 088 SAS'),
    (89, 'DEMO-CO-089', 'Alimentos de la Sabana Demo 089 SAS'),
    (90, 'DEMO-CO-090', 'Suministros del Llano Demo 090 SAS'),
    (91, 'DEMO-CO-091', 'Consultoria Integral Demo 091 SAS'),
    (92, 'DEMO-CO-092', 'Manufacturas del Centro Demo 092 SAS'),
    (93, 'DEMO-CO-093', 'Agroinsumos Nacionales Demo 093 SAS'),
    (94, 'DEMO-CO-094', 'Equipos Industriales Demo 094 SAS'),
    (95, 'DEMO-CO-095', 'Papeleria Empresarial Demo 095 SAS'),
    (96, 'DEMO-CO-096', 'Textiles Regionales Demo 096 SAS'),
    (97, 'DEMO-CO-097', 'Ferreteria Mayorista Demo 097 SAS'),
    (98, 'DEMO-CO-098', 'Servicios Ambientales Demo 098 SAS'),
    (99, 'DEMO-CO-099', 'Productos del Campo Demo 099 SAS'),
    (100, 'DEMO-CO-100', 'Comercio Digital Demo 100 SAS');

    DECLARE @Ubicaciones TABLE (
        Posicion bigint PRIMARY KEY,
        DepartamentoCodigo int NOT NULL,
        CiudadCodigo int NOT NULL
    );

    ;WITH CiudadesPorDepartamento AS (
        SELECT d.DptColCodigoDane AS DepartamentoCodigo,
               c.DvsPltColCodigoDane AS CiudadCodigo,
               ROW_NUMBER() OVER (
                   PARTITION BY d.DptColCodigoDane ORDER BY c.DvsPltColCodigoDane
               ) AS PosicionEnDepartamento
        FROM dbo.Pais p
        JOIN dbo.DepartamentosColombia d ON d.DptColPaisCodigo = p.PaisCodigo
        JOIN dbo.DivisionPoliticaColombia c ON c.DvsPltColDptColCodigoDane = d.DptColCodigoDane
        WHERE p.PaisCodigo = 170 AND p.Active = 1 AND d.Active = 1 AND c.Active = 1
    )
    INSERT INTO @Ubicaciones (Posicion, DepartamentoCodigo, CiudadCodigo)
    SELECT ROW_NUMBER() OVER (ORDER BY PosicionEnDepartamento, DepartamentoCodigo),
           DepartamentoCodigo, CiudadCodigo
    FROM CiudadesPorDepartamento;

    DECLARE @CantidadUbicaciones int = (SELECT COUNT(*) FROM @Ubicaciones);
    IF @CantidadUbicaciones = 0
        THROW 50001, 'No hay ubicaciones activas para Colombia. No se insertaron clientes.', 1;

    IF EXISTS (
        SELECT 1 FROM @Clientes lote
        JOIN dbo.Cliente existente
          ON existente.ClnTpoIdnId = 2 AND existente.ClnNumeroIdentificacion = lote.Identificacion
        WHERE existente.ClnPaisCodigo <> 170 OR existente.ClnRazonSocial <> lote.RazonSocial
    )
        THROW 50002, 'Una identificacion del lote pertenece a otro cliente. No se modificaron datos.', 1;

    DECLARE @FechaCreacion datetime = GETUTCDATE();
    INSERT INTO dbo.Cliente (
        ClnTpoIdnId, ClnNumeroIdentificacion, ClnRazonSocial,
        ClnPaisCodigo, ClnDptColCodigoDane, ClnDvsPltColCodigoDane,
        Active, CreationDate, UpdatedDate
    )
    SELECT 2, lote.Identificacion, lote.RazonSocial,
           170, ubicacion.DepartamentoCodigo, ubicacion.CiudadCodigo,
           1, @FechaCreacion, NULL
    FROM @Clientes lote
    JOIN @Ubicaciones ubicacion ON ubicacion.Posicion = ((lote.Numero - 1) % @CantidadUbicaciones) + 1
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Cliente existente
        WHERE existente.ClnTpoIdnId = 2 AND existente.ClnNumeroIdentificacion = lote.Identificacion
    );

    DECLARE @Insertados int = @@ROWCOUNT;
    COMMIT TRANSACTION;

    SELECT @Insertados AS ClientesInsertados, 100 - @Insertados AS ClientesDelLoteYaExistentes,
           (SELECT COUNT(*) FROM dbo.Cliente) AS ClientesTotales;
    SELECT COUNT(*) AS ClientesDelLote,
           COUNT(DISTINCT c.ClnDptColCodigoDane) AS Departamentos,
           COUNT(DISTINCT c.ClnDvsPltColCodigoDane) AS Municipios
    FROM dbo.Cliente c
    JOIN @Clientes lote ON c.ClnTpoIdnId = 2 AND c.ClnNumeroIdentificacion = lote.Identificacion;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
    THROW;
END CATCH;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

