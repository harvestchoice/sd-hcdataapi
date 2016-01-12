/*********************************************************************
	HC_DB_WEB Database Data Model Redesign

	****Reevaluate

*********************************************************************/
/*********************************************************************
	CELL_VALUES Indexes
*********************************************************************/
-- Evaluate CELL_VALUES fragmentation 
SELECT a.index_id, name, avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats (DB_ID(N'HC_DB_WEB_2'), OBJECT_ID(N'dbo.CELL_VALUES'), NULL, NULL, NULL) AS a
    JOIN sys.indexes AS b ON a.object_id = b.object_id AND a.index_id = b.index_id; 
GO

-- Reorganize the PK_CELL_VALUES_ID index on the CELL_VALUES table. 
USE [HC_DB_WEB_2]; 
GO
ALTER INDEX PK_CELL_VALUES_ID ON dbo.[CELL_VALUES]
REORGANIZE; 
GO

-- Create new non-clustered index
USE [HC_DB_WEB_2]
GO
CREATE NONCLUSTERED INDEX [IDX_CELL_VALUES_column_name] ON [dbo].[CELL_VALUES] (
	[column_name] ASC
)
INCLUDE ([CELL5M],[value]) WITH (SORT_IN_TEMPDB = ON)
GO
/*********************************************************************
	indicator_metadata Indexes
*********************************************************************/
-- Evaluate indicator_metadata fragmenation
SELECT a.index_id, name, avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats (DB_ID(N'HC_DB_WEB_2'), OBJECT_ID(N'dbo.indicator_metadata'), NULL, NULL, NULL) AS a
    JOIN sys.indexes AS b ON a.object_id = b.object_id AND a.index_id = b.index_id; 
GO

-- Reorganize the PK_indicator_metadata_ID index on the indicator_metadata table. 
USE [HC_DB_WEB_2]; 
GO
ALTER INDEX PK_indicator_metadata_ID ON dbo.[indicator_metadata]
REORGANIZE; 
GO

-- Create new non-clustered index
USE [HC_DB_WEB_2]
GO
CREATE NONCLUSTERED INDEX [IDX_indicator_metadata_cats]
ON [dbo].[indicator_metadata] ([published])
INCLUDE ([cat1],[cat2],[cat3])
GO
/*********************************************************************
	CELL5M Indexes
*********************************************************************/
-- Evaluate indicator_metadata fragmenation
SELECT a.index_id, name, avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats (DB_ID(N'HC_DB_WEB_2'), OBJECT_ID(N'dbo.CELL5M'), NULL, NULL, NULL) AS a
    JOIN sys.indexes AS b ON a.object_id = b.object_id AND a.index_id = b.index_id; 
GO

-- Reorganize the PK_indicator_metadata_ID index on the indicator_metadata table. 
USE [HC_DB_WEB_2]; 
GO
ALTER INDEX PK_CELL5M ON dbo.CELL5M
REORGANIZE; 
GO