USE [CorrecolTest];
SET NOCOUNT ON;

SELECT s.name AS SchemaName, t.name AS TableName, SUM(p.rows) AS [RowCount]
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
GROUP BY s.name, t.name ORDER BY s.name, t.name;

SELECT s.name AS SchemaName, t.name AS TableName, c.column_id, c.name AS ColumnName,
       ty.name AS SqlType, c.max_length, c.precision, c.scale, c.is_nullable, c.is_identity,
       dc.definition AS DefaultDefinition
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
LEFT JOIN sys.default_constraints dc ON dc.object_id = c.default_object_id
ORDER BY s.name, t.name, c.column_id;

SELECT OBJECT_SCHEMA_NAME(f.parent_object_id) AS SchemaName,
       OBJECT_NAME(f.parent_object_id) AS TableName, f.name AS ForeignKeyName,
       COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
       OBJECT_NAME(f.referenced_object_id) AS ReferencedTable,
       COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys f
JOIN sys.foreign_key_columns fc ON fc.constraint_object_id = f.object_id
ORDER BY TableName, ForeignKeyName, fc.constraint_column_id;

SELECT OBJECT_NAME(i.object_id) AS TableName, i.name AS IndexName, i.is_primary_key,
       i.is_unique, c.name AS ColumnName, ic.key_ordinal
FROM sys.indexes i
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE OBJECTPROPERTY(i.object_id, 'IsUserTable') = 1
ORDER BY TableName, IndexName, ic.key_ordinal;

SELECT type_desc, SCHEMA_NAME(schema_id) AS SchemaName, name
FROM sys.objects WHERE is_ms_shipped = 0 AND type IN ('P', 'V', 'TR', 'FN', 'IF', 'TF')
ORDER BY type_desc, name;
