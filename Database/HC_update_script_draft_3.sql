/*
Deployment script for 
HC_WEB_DB_2

This code was generated by a tool.
Changes to this file may cause incorrect behavior and will be lost if
the code is regenerated.


please set query SQLCMD mode to on.


*/

GO
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;


GO
:setvar DatabaseName "HC_WEB_DB_2"
:setvar DefaultFilePrefix "HC_WEB_DB_2"
:setvar DefaultDataPath "C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQL2012\MSSQL\DATA\"
:setvar DefaultLogPath "C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQL2012\MSSQL\Log\"

GO
:on error exit
GO
/*
Detect SQLCMD mode and disable script execution if SQLCMD mode is not supported.
To re-enable the script after enabling SQLCMD mode, execute the following:
SET NOEXEC OFF; 
*/
:setvar __IsSqlCmdEnabled "True"
GO
IF N'$(__IsSqlCmdEnabled)' NOT LIKE N'True'
    BEGIN
        PRINT N'SQLCMD mode must be enabled to successfully execute this script.';
        SET NOEXEC ON;
    END


GO
USE [$(DatabaseName)];


GO
PRINT N'Starting rebuilding table [dbo].[indicator_metadata]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [dbo].[tmp_ms_xx_indicator_metadata] (
    [id]                  INT            NULL,
    [column_name]         NVARCHAR (MAX) NULL,
    [micro_label]         NVARCHAR (MAX) NULL,
    [short_label]         NVARCHAR (MAX) NULL,
    [full_label]          NVARCHAR (MAX) NULL,
    [long_description]    NVARCHAR (MAX) NULL,
    [category_1]          NVARCHAR (MAX) NULL,
    [category_2]          NVARCHAR (MAX) NULL,
    [category_3]          NVARCHAR (MAX) NULL,
    [item]                NVARCHAR (MAX) NULL,
    [unit]                NVARCHAR (MAX) NULL,
    [decimal_places]      INT            NULL,
    [year]                NVARCHAR (MAX) NULL,
    [end_year]            INT            NULL,
    [extent]              NVARCHAR (MAX) NULL,
    [classification_type] NVARCHAR (MAX) NULL,
    [agg_type]            NVARCHAR (MAX) NULL,
    [agg_formula]         NVARCHAR (MAX) NULL,
    [table_name]          NVARCHAR (MAX) NULL,
    [column_label]        NVARCHAR (MAX) NULL,
    [table_unique_id]     NVARCHAR (MAX) NULL,
    [source]              NVARCHAR (MAX) NULL,
    [micro_source]        NVARCHAR (MAX) NULL,
    [data_worker]         NVARCHAR (MAX) NULL,
    [createdby]           NVARCHAR (MAX) NULL,
    [createddate]         NVARCHAR (MAX) NULL,
    [published]           BIT            NULL,
    [validated]           NVARCHAR (MAX) NULL,
    [last_processed]      NVARCHAR (MAX) NULL,
    [processing_comment]  NVARCHAR (MAX) NULL,
    [version]             NVARCHAR (MAX) NULL,
    [caveat]              NVARCHAR (MAX) NULL,
    [genRaster]           BIT            NULL,
    [isDomain]            BIT            NULL,
    [mxdName]             NVARCHAR (MAX) NULL,
    [classColors]         NVARCHAR (MAX) NULL,
    [classBreaks]         NVARCHAR (MAX) NULL,
    [classLabels]         NVARCHAR (MAX) NULL
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [dbo].[indicator_metadata])
    BEGIN
        INSERT INTO [dbo].[tmp_ms_xx_indicator_metadata] ([id], [column_name], [micro_label], [short_label], [full_label], [long_description], [category_1], [category_2], [category_3], [item], [unit], [decimal_places], [year], [end_year], [extent], [classification_type], [agg_type], [agg_formula], [table_name], [column_label], [table_unique_id], [source], [micro_source], [data_worker], [createdby], [createddate], [published], [validated], [last_processed], [processing_comment], [version], [caveat], [genRaster], [isDomain], [mxdName], [classColors], [classBreaks], [classLabels])
        SELECT [id],
               [column_name],
               [micro_label],
               [short_label],
               [full_label],
               [long_description],
               [category_1],
               [category_2],
               [category_3],
               [item],
               [unit],
               [decimal_places],
               [year],
               [end_year],
               [extent],
               [classification_type],
               [agg_type],
               [agg_formula],
               [table_name],
               [column_label],
               [table_unique_id],
               [source],
               [micro_source],
               [data_worker],
               [createdby],
               [createddate],
               [published],
               [validated],
               [last_processed],
               [processing_comment],
               [version],
               [caveat],
               [genRaster],
               [isDomain],
               [mxdName],
               [classColors],
               [classBreaks],
               [classLabels]
        FROM   [dbo].[indicator_metadata];
    END

DROP TABLE [dbo].[indicator_metadata];

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_indicator_metadata]', N'indicator_metadata';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Creating [dbo].[cell_values_new]...';


GO
CREATE TABLE [dbo].[cell_values_new] (
    [ID]          INT            IDENTITY (1, 1) NOT NULL,
    [CELL5M]      INT            NOT NULL,
    [column_name] NVARCHAR (MAX) NULL,
    [value]       NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_cell_value] PRIMARY KEY CLUSTERED ([ID] ASC) WITH (FILLFACTOR = 90)
);


GO
PRINT N'Creating [dbo].[cell5m]...';


GO
CREATE TABLE [dbo].[cell5m] (
    [CELL5M] INT              NOT NULL,
    [shape]  [sys].[geometry] NULL,
    CONSTRAINT [PK_CELL5M] PRIMARY KEY CLUSTERED ([CELL5M] ASC) WITH (FILLFACTOR = 90)
);


GO
PRINT N'Creating FK_cell5m_cell5m...';


GO
ALTER TABLE [dbo].[cell_values_new] WITH NOCHECK
    ADD CONSTRAINT [FK_cell5m_cell5m] FOREIGN KEY ([CELL5M]) REFERENCES [dbo].[cell5m] ([CELL5M]);


GO
PRINT N'Creating [dbo].[cell_values_new].[MS_Description]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Table for cell value cell5m, resulting in groupings of cell5m.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cell_values_new';


GO
PRINT N'Creating [dbo].[cell5m].[MS_Description]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Information for each cell5m.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cell5m';


GO
PRINT N'Refreshing [dbo].[udf_SelectSummaryVariables]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[udf_SelectSummaryVariables]';


GO
PRINT N'Refreshing [dbo].[udf_SelectVariableCollections]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[udf_SelectVariableCollections]';


GO
PRINT N'Checking existing data against newly created constraints';


GO
USE [$(DatabaseName)];


GO
ALTER TABLE [dbo].[cell_values_new] WITH CHECK CHECK CONSTRAINT [FK_cell5m_cell5m];


GO
PRINT N'Update complete.';


GO
