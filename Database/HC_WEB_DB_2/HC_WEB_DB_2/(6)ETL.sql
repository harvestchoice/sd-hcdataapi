/*********************************************************************
	HC_DB_WEB Database Data Model Redesign

		* ETL (HCTEAM13.schef to MSSQL2012.HC_WEB_DB_2)

*********************************************************************/
/*********************************************************************
	STEP #1: Copy Table Procedure	
*********************************************************************/
	/****************************************************************
		Drop Procedure
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	DROP PROCEDURE dbo.udf_copy_table
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

	CREATE PROCEDURE udf_copy_table	
		@tableName AS nvarchar(max)
	AS
	BEGIN	
		DECLARE @sqlcmd AS nvarchar(max),
				@tmpTable AS nvarchar(max),
				@table AS nvarchar(max)
		SET NOCOUNT ON;
		SET @tmpTable = 'dbo.' + @tableName + '_tmp';
		SET @table = 'dbo.' + @tableName;
		-- delete if exists
		IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(@tmpTable) AND TYPE IN (N'U'))
		BEGIN
			SET @sqlcmd = 'DROP TABLE ' + @tmpTable;
			EXEC (@sqlcmd);
		END
		-- create
		SET @sqlcmd = 'SELECT * INTO ' + @tmpTable + ' FROM ' + @table;
		EXEC (@sqlcmd);
	END
	GO
	/****************************************************************
		Test Procedure
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	-- EXECUTE 
	DECLARE @RC int
	DECLARE @tableName nvarchar(max)

	SET @tableName = 'indicator_metadata'

	EXECUTE @RC = dbo.udf_copy_table
	   @tableName
	GO
/*********************************************************************
	STEP #2: Migrate Indicators from Source
*********************************************************************/
	/****************************************************************
		Drop Procedure
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	DROP PROCEDURE dbo.udf_etl_indicators
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
	CREATE PROCEDURE udf_etl_indicators	
	AS
	BEGIN	
		DECLARE @sqlcmd AS nvarchar(max),
				@columnName AS nvarchar(max),
				@tableName AS nvarchar(max),
				@dataType AS nvarchar(max),
				@maxLength AS int,
				@cursor CURSOR
		SET NOCOUNT ON;
		-- truncate table
		TRUNCATE TABLE dbo.indicator_metadata;
		-- insert
		INSERT INTO dbo.indicator_metadata
			   ([vi_id],[varCode],[varLabel],[published],[tbName],[version],[tbKey]
			   ,[varTitle],[varDesc],[unit],[dec],[year],[yearEnd],[cat1],[cat2],[cat3]
			   ,[extent],[type],[aggType],[aggFun],[owner],[caveat],[sources],[sourceMini]
			   ,[genRaster],[isDomain],[isProduct],[mxdName],[classColors],[classBreaks],[classLabels])
		SELECT [id],[varCode],[varLabel],[published],[tbName],[version],[tbKey]
			   ,[varTitle],[varDesc],[unit],[dec],[year],[yearEnd],[cat1],[cat2],[cat3]
			   ,[extent],[type],[aggType],[aggFun],[owner],[caveat],[sources],[sourceMini]
			   ,[genRaster],[isDomain],[isProduct],[mxdName],[classColors],[classBreaks],[classLabels]  
		FROM [CANDACE\HCTEAM13].[schef].[dbo].[vi]
		WHERE published = 1;

		-- update for data type
		SET @cursor = CURSOR LOCAL FAST_FORWARD FOR 
			SELECT varCode, tbName
			FROM dbo.indicator_metadata
		OPEN @cursor
		FETCH NEXT
		FROM @cursor INTO @columnName,@tableName
		WHILE @@FETCH_STATUS = 0
		BEGIN
			PRINT 'Table ' + @tableName + ' with column ' + @columnName;

			SELECT @maxLength = CHARACTER_MAXIMUM_LENGTH, @dataType = DATA_TYPE 
			FROM [CANDACE\HCTEAM13].[schef].dbo.[HC_ETL_DataTypes]
			WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName;
			
			IF (@maxLength IS NOT NULL AND @maxLength <> -1) 
				SET @dataType = @dataType + '(' + CONVERT(VARCHAR(5), @maxLength) + ')';
			IF (@maxLength = -1)
				SET @dataType = @dataType + '(max)';			

			UPDATE [HC_DB_WEB_2].[dbo].indicator_metadata SET dataType = @dataType
			WHERE varCode = @columnName; 
			FETCH NEXT
			FROM @cursor INTO @columnName,@tableName
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

	EXECUTE @RC = dbo.udf_etl_indicators
	GO	

/*********************************************************************
	STEP #3: Delete Table Procedure	
*********************************************************************/
	/****************************************************************
		Drop Procedure
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	DROP PROCEDURE dbo.udf_delete_table
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

	CREATE PROCEDURE udf_delete_table	
		@tableName AS nvarchar(max)
	AS
	BEGIN	
		DECLARE @sqlcmd AS nvarchar(max),
				@table AS nvarchar(max)
		SET NOCOUNT ON;
		SET @table = 'dbo.' + @tableName;
		-- delete if exists
		IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(@table) AND TYPE IN (N'U'))
		BEGIN
			SET @sqlcmd = 'DROP TABLE ' + @table;
			EXEC (@sqlcmd);
		END		
	END
	GO
	/****************************************************************
		Test Procedure
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	-- EXECUTE 
	DECLARE @RC int
	DECLARE @tableName nvarchar(max)

	SET @tableName = 'indicator_metadata_tmp'

	EXECUTE @RC = dbo.udf_delete_table
	   @tableName
	GO

/*********************************************************************
	STEP #4: Migrate Cell Values for Indicators from Source
*********************************************************************/
	/****************************************************************
		Drop Procedure
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	DROP PROCEDURE dbo.udf_etl_cellValues
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
	CREATE PROCEDURE udf_etl_cellValues	
	AS
	BEGIN	
		DECLARE @sqlcmd AS nvarchar(max),
				@id AS int,
				@message AS NVARCHAR(max),
				@columnName AS nvarchar(max),
				@tableName AS nvarchar(max),
				@dataType AS nvarchar(max),
				@cursor CURSOR
		SET NOCOUNT ON;
		SET @cursor = CURSOR LOCAL FAST_FORWARD FOR 
			SELECT id, varCode, tbName, dataType
			FROM dbo.indicator_metadata 
			WHERE published = 1 AND varCode NOT IN ('CELL5M', 'CELL_VALUES', 'CELL_VALUES_old', 
				'classification', 'collection', 'collection_group', 'continuous_classification',
				'country','country_collection','discrete_classification','domain_country_results',
				'domain_variable','drupal_metadata','indicator_metadata','schema','schema_domain',
				'Variable_Inventory','ETL_Audit')
		OPEN @cursor
		FETCH NEXT
		FROM @cursor INTO @id,@columnName,@tableName,@dataType
		WHILE @@FETCH_STATUS = 0
		BEGIN
			--SET @message = 'Processing indicator table number ' + CONVERT(VARCHAR(5), @id) + ' ---> ' + @columnName;
			--RAISERROR (@message, 0, 1) WITH NOWAIT;

			-- drop if table exists 
			IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(@columnName) AND TYPE IN (N'U'))
			BEGIN
				SET @sqlcmd = 'DROP TABLE ' + @columnName;
				EXEC (@sqlcmd);
			END	

			-- update the ELT Audit table
			IF NOT EXISTS (SELECT * FROM dbo.ETL_Audit WHERE [indicator] = @columnName)
			BEGIN
				INSERT INTO [dbo].[ETL_Audit]([indicator],[created],[updated])VALUES(@columnName, GETDATE(), GETDATE());
			END					
           
		    -- create table
			SET @sqlcmd = 'CREATE TABLE ' + @columnName + '( ' + 'CELL5M INT NOT NULL PRIMARY KEY, ' +
							@columnName + ' ' + @dataType + ' NULL)';
			EXEC (@sqlcmd);

			-- load table
			SET @sqlcmd = 'INSERT ' + @columnName + ' SELECT CELL5M, ' + @columnName + 
						' FROM  [CANDACE\HCTEAM13].[schef].dbo.' + @tableName;			
			EXEC (@sqlcmd);
			UPDATE dbo.ETL_Audit SET updated = GETDATE(), row_count = @@ROWCOUNT WHERE indicator = @columnName;

			-- create index
			IF(@dataType != 'text' AND @dataType != 'nvarchar(max)' AND @dataType != 'varchar(max)')
			BEGIN
				SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @columnName + ' ON ' + @columnName + ' ( ' + @columnName + ' ASC )INCLUDE ([CELL5M]) WITH (SORT_IN_TEMPDB = ON)';
				EXEC (@sqlcmd);
			END		

			FETCH NEXT
			FROM @cursor INTO @id,@columnName,@tableName,@dataType
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

	EXECUTE @RC = dbo.udf_etl_cellValues
	GO	

/*********************************************************************
	STEP #5: Email ETL Report
*********************************************************************/
	/****************************************************************
		Drop Procedure
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	DROP PROCEDURE dbo.udf_etl_report
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
	CREATE PROCEDURE udf_etl_report	
		@emails as NVARCHAR(MAX)
	AS
	BEGIN	
		DECLARE @tableHTML  NVARCHAR(MAX) ;

		SET @tableHTML =
			N'<H1>Harvest Choice ELT Report - ' + CONVERT(VARCHAR(50), GETDATE()) + '</H1>' +
			N'<table border="1">' +
			N'<tr><th>Indicator</th><th>Number of Rows</th>' +
			N'<th>Created Date</th><th>Updated Date</th></tr>' +
			CAST ( ( SELECT td = indicator,       '',
							td = row_count, '',
							td = created, '',
							td = updated
					  FROM dbo.ETL_Audit					  
					  ORDER BY indicator ASC
					  FOR XML PATH('tr'), TYPE 
			) AS NVARCHAR(MAX) ) +
			N'</table>' ;

		EXEC msdb.dbo.sp_send_dbmail @recipients=@emails,
			@profile_name = 'Harvest Choice DBA',
			@subject = 'Havest Choice ELT Report',
			@body = @tableHTML,
			@body_format = 'HTML' ;
	END
	GO
	
	/****************************************************************
		Test Procedure
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	-- EXECUTE 
	DECLARE @RC int
	DECLARE @emails nvarchar(max)

	SET @emails = 'sparadee@spatialdev.com'

	EXECUTE @RC = dbo.udf_etl_report
	   @emails
	GO

