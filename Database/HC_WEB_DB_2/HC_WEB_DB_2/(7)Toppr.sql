/*********************************************************************
	STEP #1: Create Toppr Tables
*********************************************************************/	
	/****************************************************************
		Drop Procedure
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	DROP PROCEDURE dbo.udf_toppr
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
	CREATE PROCEDURE udf_toppr
	AS
	BEGIN	
		-- three target categories to process
		DECLARE @categories VARCHAR(MAX) = 'Harvested Area;Production;Value of Production;',
				@topprTables VARCHAR(MAX) = 'toppr_area;toppr_prod;toppr_value;', -- needs to have same number of values as @categories
				@requiredTables VARCHAR(MAX) = 'GAUL_2008_1;GAUL_2012_1;area_crop;vp_crop;', 
				@delimiter CHAR = ';',
				@category VARCHAR(MAX),
				@topprTable VARCHAR(MAX),
				@table VARCHAR(MAX),
				@indicator VARCHAR(MAX),
				@select VARCHAR(MAX) = 'SELECT ISO3.CELL5M,ISO3.ISO3',
				@from VARCHAR(MAX) = ' FROM ISO3',
				@cropSelect VARCHAR(MAX),
				@cropFrom VARCHAR(MAX),
				@sqlcmd VARCHAR(MAX),
				@cursor CURSOR

		-- loop through required tables and create base sql statement
		WHILE LEN(@requiredTables) > 0
		BEGIN
		 SET @table = LTRIM(SUBSTRING(@requiredTables, 1, CHARINDEX(@delimiter, @requiredTables) - 1));
		 SET @requiredTables = SUBSTRING(@requiredTables, CHARINDEX(@delimiter, @requiredTables) + 1, LEN(@requiredTables))
		 SET @select += ',' + @table + '.' + @table;
		 SET @from += ' LEFT JOIN ' + @table + ' ON ISO3.CELL5M = ' + @table + '.CELL5M';		 		 
		END
		PRINT @select + @from;
		-- loop through indicators for each category
		WHILE LEN(@categories) > 0
		BEGIN
		 SET @category = LTRIM(SUBSTRING(@categories, 1, CHARINDEX(@delimiter, @categories) - 1));
		 SET @categories = SUBSTRING(@categories, CHARINDEX(@delimiter, @categories) + 1, LEN(@categories))
		 SET @topprTable = LTRIM(SUBSTRING(@topprTables, 1, CHARINDEX(@delimiter, @topprTables) - 1));
		 SET @topprTables = SUBSTRING(@topprTables, CHARINDEX(@delimiter, @topprTables) + 1, LEN(@topprTables))
		 -- delete if exists
		 IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(@topprTable) AND TYPE IN (N'U'))
		 BEGIN
			SET @sqlcmd = 'DROP TABLE ' + @topprTable;
			EXEC (@sqlcmd);
		 END
		 -- create table
		 SET @sqlcmd = 'CREATE TABLE [dbo].' + @topprTable + ' ( ' +
			'[ID]			INT				IDENTITY (1, 1) NOT NULL, ' +
			'[CELL5M]		INT				NOT NULL, ' +
			'[ISO3]			NVARCHAR(3)		NULL, ' +
			'[GAUL_2008_1]	NVARCHAR(100)	NULL, ' +
			'[GAUL_2012_1]	NVARCHAR(100)	NULL, ' +
			'[area_crop]	FLOAT			NULL, ' +
			'[vp_crop]		FLOAT			NULL, ' +
			'[crop]			NVARCHAR (30)	NULL, ' +
			'[value]		FLOAT			NULL, ' +
			'CONSTRAINT [PK_' + @topprTable + '_ID] PRIMARY KEY CLUSTERED ([ID]));';
		 EXEC (@sqlcmd);
		 PRINT @category + ' --------------------->';
		 SET NOCOUNT ON;
		 SET @cursor = CURSOR LOCAL FAST_FORWARD FOR
			SELECT [varCode]      
				FROM [HC_DB_WEB_2].[dbo].[indicator_metadata]
				WHERE isProduct = 1 and cat2 = @category
		OPEN @cursor
		FETCH NEXT
		FROM @cursor INTO @indicator
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @cropSelect = @select + ',''' + @indicator + ''',' + @indicator + '.' + @indicator;
			SET @cropFrom = @from + ' LEFT JOIN ' + @indicator + ' ON ISO3.CELL5M = ' + @indicator + '.CELL5M'
			-- insert crop
			SET @sqlcmd = 'INSERT INTO ' + @topprTable + ' (CELL5M,ISO3,GAUL_2008_1,GAUL_2012_1,area_crop,vp_crop,crop,value) ' + 
						  @cropSelect + @cropFrom;
			PRINT 'INSERT --------------------->' + @sqlcmd;
			EXEC (@sqlcmd);			
			FETCH NEXT
			FROM @cursor INTO @indicator
		END		
		-- indexes	
		IF @category = 'harvested area'			
			BEGIN
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_ISO3 ' +
					  'ON [dbo].' + @topprTable + ' ([ISO3]) INCLUDE ([area_crop],[crop],[value])';
			EXEC (@sqlcmd);
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_GAUL_2008_1 ' +
					  'ON [dbo].' + @topprTable + ' ([GAUL_2008_1]) INCLUDE ([area_crop],[crop],[value])';
			EXEC (@sqlcmd);
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_GAUL_2012_1 ' +
					  'ON [dbo].' + @topprTable + ' ([GAUL_2012_1]) INCLUDE ([area_crop],[crop],[value])';
			EXEC (@sqlcmd);
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_CELL5M ' +
					  'ON [dbo].' + @topprTable + ' ([CELL5M]) INCLUDE ([area_crop],[crop],[value])';
			EXEC (@sqlcmd);
			END
		ELSE IF @category = 'production'			
			BEGIN
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_ISO3 ' +
					  'ON [dbo].' + @topprTable + ' ([ISO3]) INCLUDE ([crop],[value])';
			EXEC (@sqlcmd);
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_GAUL_2008_1 ' +
					  'ON [dbo].' + @topprTable + ' ([GAUL_2008_1]) INCLUDE ([crop],[value])';
			EXEC (@sqlcmd);
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_GAUL_2012_1 ' +
					  'ON [dbo].' + @topprTable + ' ([GAUL_2012_1]) INCLUDE ([crop],[value])';
			EXEC (@sqlcmd);
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_CELL5M ' +
					  'ON [dbo].' + @topprTable + ' ([CELL5M]) INCLUDE ([crop],[value])';
			EXEC (@sqlcmd);
			END
		ELSE IF @category = 'value of production'			
			BEGIN
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_ISO3 ' +
					  'ON [dbo].' + @topprTable + ' ([ISO3]) INCLUDE ([vp_crop],[crop],[value])';
			EXEC (@sqlcmd);				
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_GAUL_2008_1 ' +
						  'ON [dbo].' + @topprTable + ' ([GAUL_2008_1]) INCLUDE ([vp_crop],[crop],[value])';
			EXEC (@sqlcmd);				
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_GAUL_2012_1 ' +
						  'ON [dbo].' + @topprTable + ' ([GAUL_2012_1]) INCLUDE ([vp_crop],[crop],[value])';
			EXEC (@sqlcmd);	
			SET @sqlcmd = 'CREATE NONCLUSTERED INDEX IDX_' + @topprTable + '_CELL5M ' +
					  'ON [dbo].' + @topprTable + ' ([CELL5M]) INCLUDE ([vp_crop],[crop],[value])';
			EXEC (@sqlcmd);
			END
						
		CLOSE @cursor
		DEALLOCATE @cursor				 
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

	EXECUTE @RC = dbo.udf_toppr
	GO	
