USE DependencyAnalysis
GO

DROP TABLE Dependencies
GO

CREATE TABLE Dependencies
(
	[id] INT IDENTITY,
	[source_entity_name] NVARCHAR(257),
	[referenced_entity_name] NVARCHAR(257),
	[referenced_minor_name] NVARCHAR(257) NULL,
	[referenced_class_desc] NVARCHAR(257),
	[is_updated] BIT,
	[is_selected] BIT
);
GO

DROP TABLE Updates
GO

CREATE TABLE Updates
(
	[id] INT IDENTITY,
	[StoredProcedure] NVARCHAR(257),
	[Table] NVARCHAR(257),
	[Type] CHAR(1)
);
GO