/*********************************************************************
	HC_DB_WEB Database Data Model Redesign

	Steps
	=====
	1. Create a copy of the current development database (backup & restore)
	2. Rename current CELL_VALUES and indicator_metadata as _old
	3. Create new tables for CELL_VALUES, CELL5M and indicator_metadata
*********************************************************************/

/*********************************************************************
	STEP #1 - Create copy of the current development database 
			 (backup & restore)
	READ COMMENTS FOR EXECUTION INSTRUCTIONS!!!
*********************************************************************/
-- Get a backup of the current development database
-- Update the <Backup Path> to the path of the backup
USE HC_WEB_DB_DEV;
GO
BACKUP DATABASE HC_WEB_DB_DEV
TO DISK = '<Backup Path>\HC_WEB_DB_DEV.Bak'
   WITH FORMAT,
      MEDIANAME = 'HC_WEB_DB_DEV',
      NAME = 'Full Backup of HC_WEB_DB_DEV';
GO

-- Using the returned values update the following restore statement before executing 
-- Update the <Backup Path> to the path of the backup
RESTORE FILELISTONLY
   FROM DISK = '<Backup Path>\HC_WEB_DB_DEV.Bak';

-- Using the 'LogicalName' values in the previous statement update the <Data> and <Log>
-- values appropriately (Type = D for data and Type = L for log)
-- Update the <Data Path> to the current SQL Server Instance Data path
-- Update the <Backup Path> to the path of the backup
-- Appx. execution time: 1.5 hours
USE master;
GO
RESTORE DATABASE HC_DB_WEB_2
   FROM DISK = '<Backup Path>\HC_WEB_DB_DEV.Bak'
   WITH REPLACE, RECOVERY,
   MOVE '<Data>' TO '<Data Path>\HC_WEB_DB_2.mdf', 
   MOVE '<Log>' TO '<Data Path>\HC_WEB_DB_2.ldf'
GO

/*********************************************************************
	STEP #2 - Rename current CELL_VALUES and indicator_metadata as _old
	Instructions: Run script set together
*********************************************************************/
USE [HC_DB_WEB_2]
GO
-- Rename the current CELL_VALUES table to CELL_VALUES_old
sp_rename CELL_VALUES, CELL_VALUES_old;
GO
-- Rename the current indicator_metadata table to indicator_metadata_old
sp_rename indicator_metadata, indicator_metadata_old;
GO

/*********************************************************************
	STEP #3 - Create new tables for CELL_VALUES, CELL5M and 
			 indicator_metadata
*********************************************************************/
USE [HC_DB_WEB_2]
GO
-- Create new ETL_Audit Table
CREATE TABLE [dbo].[ETL_Audit] (
    [indicator]		nvarchar(max)	NULL,
	[row_count]		INT				NULL,
	[created]		Datetime		NULL,
	[updated]		Datetime		NULL
);
GO
-- Create new CELL_VALUES Table
CREATE TABLE [dbo].[CELL_VALUES] (
    [ID]			INT				IDENTITY (1, 1) NOT NULL,
    [CELL5M]		INT				NOT NULL,
    [column_name]	NVARCHAR (30)	NULL,
    [value]			NVARCHAR (255)	NULL,
    CONSTRAINT [PK_CELL_VALUES_ID] PRIMARY KEY CLUSTERED ([ID])
);
GO
-- Create new CELL5M Spatial Table
CREATE TABLE [dbo].[CELL5M] (
    [CELL5M]		INT				NOT NULL,
    [Shape]			[geometry]		NULL,
    CONSTRAINT [PK_CELL5M] PRIMARY KEY CLUSTERED ([CELL5M])
);
GO
-- Create new indicator_metadata Table 
-- (Adapting same schema as origin data in HCTEAM13.schef.dbo.vi on CANDACE)
CREATE TABLE [dbo].[indicator_metadata] (
	[ID]			INT				IDENTITY (1, 1) NOT NULL,
	[vi_id]			INT				NULL,
	[varCode]		NVARCHAR(max)	NULL,
	[varLabel]		NVARCHAR(max)	NULL,
	[published]		BIT				NULL,
	[tbName]		NVARCHAR(max)	NULL,
	[version]		NVARCHAR(max)	NULL,	
	[tbKey]			NVARCHAR(max)	NULL,
	[varTitle]		NVARCHAR(max)	NULL,
	[varDesc]		NVARCHAR(max)	NULL,
	[unit]			NVARCHAR(max)	NULL,
	[dec]			INT				NULL,
	[year]			NVARCHAR(max)	NULL,
	[yearEnd]		INT				NULL,
	[cat1]			NVARCHAR(max)	NULL,
	[cat2]			NVARCHAR(max)	NULL,
	[cat3]			NVARCHAR(max)	NULL,
	[extent]		NVARCHAR(max)	NULL,
	[type]			NVARCHAR(max)	NULL,
	[aggType]		NVARCHAR(max)	NULL,
	[aggFun]		NVARCHAR(max)	NULL,
	[owner]			NVARCHAR(max)	NULL,
	[caveat]		NVARCHAR(max)	NULL,
	[sources]		NVARCHAR(max)	NULL,
	[sourceMini]	NVARCHAR(max)	NULL,
	[genRaster]		BIT				NULL,	
	[isDomain]		BIT				NULL,
	[isProduct]		BIT				NULL,	
	[mxdName]		NVARCHAR(max)	NULL,
	[classColors]	NVARCHAR(max)	NULL,
	[classBreaks]	NVARCHAR(max)	NULL,
	[classLabels]	NVARCHAR(max)	NULL,
	[dataType]		NVARCHAR(25)	NULL,
    CONSTRAINT [PK_indicator_metadata_ID] PRIMARY KEY CLUSTERED ([ID])
);
GO