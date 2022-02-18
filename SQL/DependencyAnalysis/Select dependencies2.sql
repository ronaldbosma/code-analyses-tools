USE DependencyAnalysis;

WITH dep_cte (source_entity_name, referenced_entity_name)
AS
(
	SELECT DISTINCT source_entity_name, referenced_entity_name
	FROM Dependencies
	WHERE referenced_class_desc = 'OBJECT_OR_COLUMN'
		AND is_updated = 1
)
SELECT
	  dep.source_entity_name AS [StoredProcedure]
	, dep.referenced_entity_name AS [Table]
	, 'U' AS [Type]
FROM dep_cte dep
	INNER JOIN
	(
		SELECT referenced_entity_name
		FROM dep_cte
		GROUP BY referenced_entity_name
		HAVING COUNT(*) > 1
	) AS tbl ON dep.referenced_entity_name = tbl.referenced_entity_name
	INNER JOIN
	(
		SELECT source_entity_name
		FROM dep_cte
		GROUP BY source_entity_name
		HAVING COUNT(*) > 1
	) AS sp ON dep.source_entity_name = sp.source_entity_name