/*********************************************************************
	STEP #1: Clean Up Tables Not in Use
*********************************************************************/
	/****************************************************************
		Drop Procedure
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	DROP PROCEDURE dbo.udf_delete_unused_tables
	GO
	/****************************************************************
		Create Procedure
	*****************************************************************/
	USE HC_DB_WEB_2
	GO
	SET ANSI_NULLS ON
	GO
	SET QUOTED_IDENTIFIER ON
	GO
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	GO
	CREATE PROCEDURE udf_delete_unused_tables	
	AS
	BEGIN	
		DECLARE @sqlcmd AS nvarchar(max),
				@tableName AS nvarchar(max),
				@cursor CURSOR
		SET NOCOUNT ON;
		SET @cursor = CURSOR LOCAL FAST_FORWARD FOR
			SELECT DISTINCT TABLE_NAME 
			FROM INFORMATION_SCHEMA.COLUMNS	
			WHERE TABLE_NAME NOT IN(SELECT varCode
			FROM [HC_DB_WEB_2].[dbo].indicator_metadata) 
			AND TABLE_NAME NOT IN ('CELL5M', 'CELL_VALUES', 'CELL_VALUES_old', 
						'classification', 'collection', 'collection_group', 'continuous_classification',
						'country','country_collection','discrete_classification','domain_country_results',
						'domain_variable','drupal_metadata','indicator_metadata','schema','schema_domain',
						'Variable_Inventory','GAUL_2008_0','GAUL_2012_0','ETL_Audit','indicator_metadata_old',
						'indicator_metadata_r2','tmp_build_cell_values','vCountryList','domain_6_MarketAccess',
						'toppr_area','toppr_prod','toppr_value')
			AND TABLE_NAME NOT IN (SELECT DISTINCT [column_name] FROM [domain_variable])
			ORDER BY TABLE_NAME
		OPEN @cursor
		FETCH NEXT
		FROM @cursor INTO @tableName
		WHILE @@FETCH_STATUS = 0
		BEGIN
			PRINT 'Deleting table --> ' + @tableName;
			SET @sqlcmd = 'DROP TABLE ' + @tableName;
			EXEC (@sqlcmd);
			FETCH NEXT
			FROM @cursor INTO @tableName
		END		
	CLOSE @cursor
	DEALLOCATE @cursor
	END
	GO
	
	/****************************************************************
		Test Procedure
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	-- EXECUTE 
	DECLARE @RC int

	EXECUTE @RC = dbo.udf_delete_unused_tables
	GO	
