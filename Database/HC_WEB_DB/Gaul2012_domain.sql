USE [HC_WEB_DB]
GO

select * from [schema];
-- add schema record (38)
INSERT INTO [dbo].[schema]([id],[description],[name],[preview_url],[createdby],[createddate],[long_description]
           ,[published],[for_mappr],[restrictions],[json_geometry])
     VALUES(38,'GAUL 2012 Region and District Boundaries','2012 Region and District Boundaries','np.jpg','sparadee'
           ,CURRENT_TIMESTAMP,'Gaul 1 and 2 boundaries.',1,1,NULL,NULL)
GO

select * from [domain_variable];
-- add domain_variable records (366 & 367)
INSERT INTO [dbo].[domain_variable] ([id],[column_name],[micro_label],[short_label],[full_label],[long_description]
           ,[category_1],[category_2],[category_3],[item],[unit],[decimal_places],[year],[end_year],[extent]
           ,[classification_type],[agg_type],[agg_formula],[table_name],[column_label],[table_unique_id]
           ,[source],[micro_source],[createdby],[createddate],[published])
     VALUES
           (366, 'GAUL_2012_1', 'GAUL Region', 'GAUL Region', 'GAUL Region', 'GAUL Level 1 region boundaries.'
           ,'Administrative','National','region', 'boundaries', 'ID', NULL, 2012, NULL, 'sub-Saharan Africa'
		   , 'class', 'NONE', NULL, 'cell5m_hcadmin', 'GAUL_2012_1',NULL, 'GAUL', NULL, 'sparadee', CURRENT_TIMESTAMP
		   ,1)
GO
INSERT INTO [dbo].[domain_variable] ([id],[column_name],[micro_label],[short_label],[full_label],[long_description]
           ,[category_1],[category_2],[category_3],[item],[unit],[decimal_places],[year],[end_year],[extent]
           ,[classification_type],[agg_type],[agg_formula],[table_name],[column_label],[table_unique_id]
           ,[source],[micro_source],[createdby],[createddate],[published])
     VALUES
           (367, 'GAUL_2012_2', 'GAUL District', 'GAUL District', 'GAUL District', 'GAUL Level 2 district boundaries.'
           ,'Administrative','National','District', 'boundaries', 'ID', NULL, 2012, NULL, 'sub-Saharan Africa'
		   , 'class', 'NONE', NULL, 'cell5m_hcadmin', 'GAUL_2012_2',NULL, 'GAUL', NULL, 'sparadee', CURRENT_TIMESTAMP
		   ,1)
GO

select * from [schema_domain];
-- add schema_domain relationship
INSERT INTO [dbo].[schema_domain] ([id], [schemaid], [domainid]) VALUES ((SELECT MAX(id) + 1 FROM  [schema_domain]), 38, 366);
INSERT INTO [dbo].[schema_domain] ([id], [schemaid], [domainid]) VALUES ((SELECT MAX(id) + 1 FROM  [schema_domain]), 38, 367);

SELECT max(id) FROM dbo.classification
-- add classification (regions) 9435- 10020
INSERT INTO [dbo].[classification]([id],[name],[createdby],[createddate])
     SELECT (select max(id) from classification) + row_number() over (order by (select NULL)),gaul.name, 'sparadee', CURRENT_TIMESTAMP
	 FROM (select distinct GAUL_2012_1 as name from CELL_VALUES where GAUL_2012_1 IS NOT NULL) as gaul
	 ORDER BY gaul.name

-- add discrete_classification (regions - 586)
INSERT INTO [dbo].[discrete_classification]([id],[originalid],[domainid],[classid],sortorder)
SELECT (select max(id) from [discrete_classification]) + row_number() over (order by (select NULL)),class.name, 366, class.id,( row_number() over (order by (select NULL)))
FROM (select * from classification where id > 9434) as class
	 ORDER BY class.name

SELECT max(id) FROM dbo.classification
-- add classification (districts) 10020 - 13697
INSERT INTO [dbo].[classification]([id],[name],[createdby],[createddate])
     SELECT (select max(id) from classification) + row_number() over (order by (select NULL)),gaul.name, 'sparadee', CURRENT_TIMESTAMP
	 FROM (select distinct GAUL_2008_2 as name from CELL_VALUES where GAUL_2008_2 IS NOT NULL) as gaul
	 ORDER BY gaul.name

-- add discrete_classification (districts - 3677)
INSERT INTO [dbo].[discrete_classification]([id],[originalid],[domainid],[classid],sortorder)
SELECT (select max(id) from [discrete_classification]) + row_number() over (order by (select NULL)),class.name, 365, class.id,( row_number() over (order by (select NULL)))
FROM (select * from classification where id > 10020) as class
	 ORDER BY class.id