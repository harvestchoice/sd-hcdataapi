USE [HC_WEB_DB]
GO

CREATE VIEW dbo.vRegionNames
 AS
SELECT row_number() over (order by (select NULL)) as id, gaul.name FROM (
SELECT DISTINCT GAUL_2008_1 as name
FROM CELL_VALUES) as gaul
GO

