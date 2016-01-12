/*********************************************************************
	HC_DB_WEB Database Data Model Redesign

		* Migrate Cell Values Data

*********************************************************************/
/*********************************************************************
	STEP #1: Create temporary table
	Create temporary table to store all the columns in CELL_VALUES to 
	be migrated (this will allow a more controlled/visual approach 
	to the load)

	Empty table:
	TRUNCATE TABLE [HC_DB_WEB_2].[dbo].[tmp_build_cell_values];
*********************************************************************/
	/****************************************************************
		Create the temparary table
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	CREATE TABLE [dbo].[tmp_build_cell_values] (
		[ID]				INT				IDENTITY (1, 1) NOT NULL,
		[select_columns]	NVARCHAR(max)	NULL,
		[unpivot_columns]	NVARCHAR(max)	NULL,
		[num_inserted_rows]	INT				NULL,
		[_count]			INT				NULL
	);
	GO
	/****************************************************************
		Populate the temporary table with CELL_VALUES_old column data
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	INSERT INTO [tmp_build_cell_values]( [select_columns], [unpivot_columns])
	SELECT 'CAST(' + quotename(c.column_name) + ' AS nvarchar(max)) AS ' + quotename(c.column_name),  quotename(c.column_name)
			 FROM information_schema.columns as c
			 WHERE c.table_name = N'CELL_VALUES_old' AND 
			COLUMN_NAME NOT IN ('CELL5M', 'Shape');
	GO
	/****************************************************************
		View the temporary table data
	*****************************************************************/
	SELECT * FROM [HC_DB_WEB_2].dbo.[tmp_build_cell_values]
	ORDER BY unpivot_columns; 
	-- 770 rows

/*********************************************************************
	STEP #2: Migrate the data from CELL_VALUES_old into new data 
	structure
*********************************************************************/
	/****************************************************************
		Create a new INDIVIDUAL data table for each column
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	DECLARE @id AS INT,
			@message AS NVARCHAR(max),
			@columnName AS NVARCHAR(max),
			@dataType nvarchar(max),
			@insert AS NVARCHAR(max),
			@index AS NVARCHAR(max),
			@create_table AS NVARCHAR(max),
			@build_cell_values CURSOR

	SET @build_cell_values = CURSOR FOR 
		SELECT	[tmp_build_cell_values].ID,
				[tmp_build_cell_values].unpivot_columns
		FROM [tmp_build_cell_values]
		-- Here you can control the number of columns processed for a single execution
		-- ID is the pk for the tmp_build_cell_values (there are 770 rows, 1-770)
		-- Appx execution time is 45 mins per 50 columns
		 WHERE [tmp_build_cell_values].ID BETWEEN 1 AND 770

	OPEN @build_cell_values
	FETCH NEXT
	FROM @build_cell_values INTO @id, @columnName
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @message = 'Processing column number ' + CONVERT(VARCHAR(5), @id) + ' ---> ' + @columnName;
			RAISERROR (@message, 0, 1) WITH NOWAIT;
		-- Get the data type
		SET @columnName = Replace(Replace(@columnName, '[', ''), ']', '')
		SET @dataType = (SELECT TOP 1 dataType FROM indicator_metadata WHERE varCode = @columnName);
		IF @dataType = 'nvarchar' BEGIN SET @dataType = @dataType + '(255)' END
		IF @dataType IS NOT NULL
		BEGIN
			SET @create_table = 'CREATE TABLE ' + @columnName + '( ' +
			'CELL5M INT NOT NULL PRIMARY KEY, ' +
			@columnName + ' ' + @dataType + ' NULL)';
			PRINT @create_table;
			EXEC (@create_table);
			IF(@dataType = 'nvarchar(255)' OR @dataType = 'text')
			BEGIN
				SET @insert = 'INSERT ' + @columnName + ' SELECT CELL5M, value as ' + @columnName + ' FROM CELL_VALUES as CV WITH (NOLOCK) ' +
				'WHERE CV.column_name = ''' + @columnName + '''';
			END
			ELSE			
				SET @insert = 'INSERT ' + @columnName + ' SELECT CELL5M,CAST(value as float) as ' + @columnName + ' FROM CELL_VALUES as CV WITH (NOLOCK) ' +
				'WHERE CV.column_name = ''' + @columnName + '''';
			EXEC (@insert);
			IF(@dataType != 'text')
			BEGIN
				SET @index = 'CREATE NONCLUSTERED INDEX IDX_' + @columnName + ' ON ' + @columnName + ' ( ' + @columnName + ' ASC )INCLUDE ([CELL5M]) WITH (SORT_IN_TEMPDB = ON)';
				EXEC (@index);
			END
		END
		FETCH NEXT
		FROM @build_cell_values INTO @id, @columnName
	END
	CLOSE @build_cell_values
	DEALLOCATE @build_cell_values
	/****************************************************************
		Create tables from columns that are in CELL_VALUES, but not
		in the indiciator_metadata table.
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	DECLARE @id AS INT,
			@message AS NVARCHAR(max),
			@columnName AS NVARCHAR(max),
			@columnType AS NVARCHAR(max),
			@dataType nvarchar(max),
			@maxLength nvarchar(max),
			@insert AS NVARCHAR(max),
			@index AS NVARCHAR(max),
			@create_table AS NVARCHAR(max),
			@build_cell_values CURSOR

	SET @build_cell_values = CURSOR FOR 
		SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH 
		FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'CELL_VALUES_old' 
		AND COLUMN_NAME NOT IN(SELECT varCode FROM [HC_DB_WEB_2].[dbo].indicator_metadata)
		AND COLUMN_NAME NOT IN('Shape')
		

	OPEN @build_cell_values
	FETCH NEXT
	FROM @build_cell_values INTO @columnName,@columnType,@maxLength
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @message = 'Processing column ---> ' + @columnName;
			RAISERROR (@message, 0, 1) WITH NOWAIT;
		-- Add maximum length if it exists
		IF @maxLength IS NOT NULL
			BEGIN
			SET @dataType = @columnType + '(' + @maxLength + ')';
			END
		ELSE
			SET @dataType = @columnType;
		IF @dataType IS NOT NULL
		BEGIN
			SET @create_table = 'CREATE TABLE ' + @columnName + '( ' +
			'CELL5M INT NOT NULL PRIMARY KEY, ' +
			@columnName + ' ' + @dataType + ' NULL)';
			PRINT @create_table;
			EXEC (@create_table);
			IF(@columnType IN ('nvarchar','text','varchar'))
			BEGIN
				SET @insert = 'INSERT ' + @columnName + ' SELECT CELL5M, value as ' + @columnName + ' FROM CELL_VALUES as CV WITH (NOLOCK) ' +
				'WHERE CV.column_name = ''' + @columnName + '''';
			END
			ELSE			
				SET @insert = 'INSERT ' + @columnName + ' SELECT CELL5M,CAST(value as ' + @columnType + ') as ' + @columnName + ' FROM CELL_VALUES as CV WITH (NOLOCK) ' +
				'WHERE CV.column_name = ''' + @columnName + '''';
			EXEC (@insert);
			IF(@dataType != 'text')
			BEGIN
				SET @index = 'CREATE NONCLUSTERED INDEX IDX_' + @columnName + ' ON ' + @columnName + ' ( ' + @columnName + ' ASC )INCLUDE ([CELL5M]) WITH (SORT_IN_TEMPDB = ON)';
				EXEC (@index);
			END
		END
		FETCH NEXT
		FROM @build_cell_values INTO @columnName,@columnType,@maxLength
	END
	CLOSE @build_cell_values
	DEALLOCATE @build_cell_values
	/****************************************************************
		Migrate all columns in to a vertical CELL_VALUES table
		(abandoned for more efficiant single table approach)
	*****************************************************************/
	--USE [HC_DB_WEB_2]
	--GO
	--DECLARE @id AS INT,
	--		@message AS NVARCHAR(max),
	--		@select_columns AS NVARCHAR(max),
	--		@unpivot_columns AS NVARCHAR(max),
	--		@select_statement AS NVARCHAR(max),
	--		@build_cell_values CURSOR

	--SET @build_cell_values = CURSOR FOR 
	--	SELECT	[tmp_build_cell_values].ID,
	--			[tmp_build_cell_values].select_columns,
	--			[tmp_build_cell_values].unpivot_columns
	--	FROM [tmp_build_cell_values]
	--	-- Here you can control the number of columns processed for a single execution
	--	-- ID is the pk for the tmp_build_cell_values (there are 770 rows, 1-770)
	--	-- Appx execution time is 45 mins per 50 columns
	--	 --WHERE [tmp_build_cell_values].ID BETWEEN 0 AND 100

	--OPEN @build_cell_values
	--FETCH NEXT
	--FROM @build_cell_values INTO @id, @select_columns, @unpivot_columns
	--WHILE @@FETCH_STATUS = 0
	--BEGIN
	--	BEGIN TRANSACTION
	--	SET @select_statement = 'SELECT CELL5M, column_name, value
	--				FROM (SELECT CELL5M, ' + @select_columns + ' FROM CELL_VALUES_old)
	--				AS Result    
	--				Unpivot(
	--					value For column_name In (' + @unpivot_columns + ')				
	--				) AS UnPvt';	
	--	SET @message = 'Processing column number ' + CONVERT(VARCHAR(5), @id) + ' ---> ' + @unpivot_columns;
	--	-- This will print out a message (in the message table of SSMS) so you can view the status
	--	-- of the execution of this logic
	--	RAISERROR (@message, 0, 1) WITH NOWAIT;
	--	-- insert the data for this column into the new CELL_VALUES table structure
	--	INSERT INTO CELL_VALUES( CELL5M, column_name, value)
	--	EXEC(@select_statement);
	--	-- update our temporary table with the total number of rows inserted for this column
	--	UPDATE [tmp_build_cell_values] SET [tmp_build_cell_values].[num_inserted_rows] = @@ROWCOUNT 
	--		WHERE [tmp_build_cell_values].[ID] = @id;
	--	COMMIT TRANSACTION
	--	FETCH NEXT
	--	FROM @build_cell_values INTO @id, @select_columns, @unpivot_columns
	--END
	--CLOSE @build_cell_values
	--DEALLOCATE @build_cell_values

