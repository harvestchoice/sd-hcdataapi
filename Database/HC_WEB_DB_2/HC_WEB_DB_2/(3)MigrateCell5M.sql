/*********************************************************************
	HC_DB_WEB Database Data Model Redesign

		* Migrate CELL5M Spatial Data

*********************************************************************/
/*********************************************************************
	Migrate data from dbo.CELL_VALUES into dbo.cell5m
	Appx. execution time: 1 minute
*********************************************************************/
USE [HC_DB_WEB_2]
GO
INSERT INTO CELL5M(CELL5M, Shape)
SELECT CELL5M, Shape FROM dbo.CELL_VALUES_old;
GO