/*********************************************************************
	HC_DB_WEB Database Data Model Redesign

		* Schef Database View

		Required to expose column data types for all tables, as the
		information_schema is not available through linked server
		access. Information is used in the ETL process for the
		HC_WEB_DB_2 database.

*********************************************************************/
USE [schef]
GO
CREATE VIEW [dbo].[HC_ETL_DataTypes] AS 

SELECT TABLE_NAME,COLUMN_NAME,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH 
FROM INFORMATION_SCHEMA.COLUMNS

GO

