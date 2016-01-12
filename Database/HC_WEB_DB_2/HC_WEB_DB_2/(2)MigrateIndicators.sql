/*********************************************************************
	HC_DB_WEB Database Data Model Redesign

		* Migrate Indicators

*********************************************************************/
/*********************************************************************
	STEP #1: Populate indicator_metadata table
	
	Migrate data from dbo.indicator_metadata_old into 
	dbo.indicator_metadata

	Empty Table: 
	TRUNCATE TABLE [HC_DB_WEB_2].[dbo].[indicator_metadata]
*********************************************************************/
	/****************************************************************
		Perform insert from indicator_metadata_old
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	INSERT INTO [dbo].[indicator_metadata]
			   ([varCode],[varLabel],[varTitle],[varDesc],[cat1],[cat2],[cat3]
			   ,[unit],[dec],[year],[yearEnd],[extent],[type],[aggType],[aggFun]
			   ,[tbName],[tbKey],[sources],[sourceMini],[owner],[published]
			   ,[version],[caveat],[genRaster],[isDomain],[mxdName],[classColors],[classBreaks],[classLabels])
	SELECT [column_name],[short_label],[full_label],[long_description],[category_1],[category_2],[category_3]
		  ,[unit],[decimal_places],[year],[end_year],[extent],[classification_type],[agg_type],[agg_formula]
		  ,[table_name],[table_unique_id],[source],[micro_source],[data_worker],[published]
		  ,[version],[caveat],[genRaster],[isDomain],[mxdName],[classColors],[classBreaks],[classLabels]
	  FROM [dbo].[indicator_metadata_old]
	GO
	/****************************************************************
		Perform insert from indicator_metadata_r2
	*****************************************************************/
	USE [HC_DB_WEB_2]
	GO
	INSERT INTO [dbo].[indicator_metadata]
			   ([varCode],[varLabel],[varTitle],[varDesc],[cat1],[cat2],[cat3]
			   ,[unit],[dec],[year],[yearEnd],[extent],[type],[aggType],[aggFun]
			   ,[tbName],[tbKey],[sources],[sourceMini],[owner],[published]
			   ,[version],[caveat],[genRaster],[isDomain],[mxdName],[classColors],[classBreaks],[classLabels])
	SELECT		[varCode],[varLabel],[varTitle],[varDesc],[cat1],[cat2],[cat3]
			   ,[unit],[dec],[year],[yearEnd],[extent],[type],[aggType],[aggFun]
			   ,[tbName],[tbKey],[sources],[sourceMini],[owner],[dPublished]
			   ,[version],[caveat],[genRaster],[isDomain],[mxdName],[classColors],[classBreaks],[classLabels]
				--,[isProduct],[format],[citation],[dAuthor],[dTopic],[dCrop],[dKeywords]
	  FROM [dbo].[indicator_metadata_r2]
	GO
/*********************************************************************
	STEP #2: Update indicator_metadata dataType field, used in migration
	logic.
*********************************************************************/
UPDATE [HC_DB_WEB_2].[dbo].indicator_metadata SET dataType = isc.DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS isc
WHERE TABLE_NAME = 'CELL_VALUES_old' AND varCode = isc.COLUMN_NAME

/*********************************************************************
	STEP #3: Evaluate indicator_metadata
*********************************************************************/
	/****************************************************************
		Fields in indicator_metadata but NOT in CELL_VALUES
	*****************************************************************/
	SELECT ID,varCode
	FROM [HC_DB_WEB_2].[dbo].indicator_metadata
	WHERE varCode NOT IN(SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'CELL_VALUES_old')
	ORDER BY varCode
	/****************************************************************
		Fields in CELL_VALUES but NOT in indicator_metadata
	*****************************************************************/
	SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'CELL_VALUES_old' AND COLUMN_NAME NOT IN(SELECT varCode
	FROM [HC_DB_WEB_2].[dbo].indicator_metadata)
	ORDER BY DATA_TYPE,COLUMN_NAME
	/****************************************************************
		List all data in CELL_VALUES_old
	*****************************************************************/
	SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CELL_VALUES_old'
	/****************************************************************
		View indicator_metadata
	*****************************************************************/
	SELECT * FROM [HC_DB_WEB_2].[dbo].[indicator_metadata];