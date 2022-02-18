USE DependencyAnalysis

SELECT DISTINCT
	  source_entity_name AS [StoredProcedure]
	, referenced_entity_name AS [Table]
	, CASE
		WHEN EXISTS (SELECT 1 FROM Dependencies WHERE source_entity_name = dep.source_entity_name AND referenced_entity_name = dep.referenced_entity_name AND is_updated = 1) THEN 'U'
		WHEN EXISTS (SELECT 1 FROM Dependencies WHERE source_entity_name = dep.source_entity_name AND referenced_entity_name = dep.referenced_entity_name AND is_selected = 1) THEN 'R'
	    ELSE ''
	  END AS [Type]
FROM Dependencies dep
WHERE referenced_class_desc = 'OBJECT_OR_COLUMN'
	AND is_updated = 1
	--AND (is_updated = 1 OR is_selected = 1)