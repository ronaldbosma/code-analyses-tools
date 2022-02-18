USE KBTPro


INSERT INTO DependencyAnalysis.dbo.Dependencies
SELECT s.[name] AS source_entity_name,
		ref.referenced_entity_name,
		ref.referenced_minor_name,
		ref.referenced_class_desc,
		ref.is_updated,
		ref.is_selected
FROM dbo.sysobjects s
	CROSS APPLY sys.dm_sql_referenced_entities  (OBJECT_SCHEMA_NAME(s.id)+'.'+s.[name], 'OBJECT') ref
WHERE s.[type] = 'P'
	--AND is_updated = 0 AND is_insert_all = 1