/**************************************************************************************************************************
	HC_DB_WEB Database Data Model Redesign

	Development Date:	April 2014

	Synopsis
	========
	The data model redesign for the HC_WEB_DB is required due to a failure in the data model design. 
	
	The CELL_VALUES table's original design resulted in horizontal table growth: an additional column for each new indicator 
	added to the table. The latest iteration of the database development requires the addition of over 300 indicators, bringing 
	the table's column total to over 700 columns. The new columns combined with the 291,892 rows and the required queries on 
	this resource caused a catastrophic failure in performance. Thus, a new data model design is required.

	The data model solution is to normalize the CELL_VALUES table into TWO tables. One table to be called CELL5M to contain the
	CELL5M primary key and geometry and one table to be called CELL_VALUES to contain the indicator name, value and CELL5M 
	foreign key in three columns allowing for vertical, normalized table growth. 

	In addition, while the CELL_VALUES table is undergoing a redesign this iteration of development will also include a redesign 
	of the indicator_metadata table, in order to purge unused columns and add new columns.

	Prerequisites & Assumptions
	===========================
	This SQL Server Project is designed to be executed on a copy of the HC_WEB_DB_DEV database (SQL Server 2012 Standard) located 
	on the development server CANDACE, in its current state as of March 29, 2014.

	Impacted Objects
	================
		1. CELL_VALUES	(table)
		2. indicator_metadata	(table)
		3. domain_6_MarketAccess	(view)
		4. udf_SelectVariableCollections	(stored procedure)
		5. udf_SelectSummaryVariables	(stored procedure)
		6. udf_addCountryDomainResults	(stored procedure)

	Instructions
	============
	Run the following scripts in order, following any internal instructions within each script:

		1.	(1)DataModelRedesign.sql
		2.	(2)DataMigration.sql
		3.  (3)UpdateDependencies.sql
		4.	(4)CleanUp.sql
		5.  (5)Validation.sql

*****************************************************************************************************************************/