/*********************************************************************

		* Migrate data from HC_DB_WEB_2 to PostgreSQL

*********************************************************************/

-- Edit FCT, Abuja
COPY gaul_2008_1
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/gaul_2008_1.csv' 
CSV;

-- Update column
UPDATE gaul_2008_1 SET gaul_2008_1 = replace(gaul_2008_1, '.', ',');

COPY gaul_2008_0
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/gaul_2008_0.csv' 
CSV;

COPY gaul_2008_2
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/gaul_2008_2.csv' 
CSV;

COPY gaul_2012_0
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/gaul_2012_0.csv' 
CSV;

COPY gaul_2012_1
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/gaul_2012_1.csv' 
CSV;

COPY gaul_2012_2
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/gaul_2012_2.csv' 
CSV;

COPY indicator_metadata
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/indicator_metadata.csv' 
CSV delimiter E'\t';

update indicator_metadata
set datatype = 'numeric'
where datatype = 'float';

update indicator_metadata
set datatype = 'character varying'
where datatype = 'nvarchar(max)';

update indicator_metadata
set datatype = 'varchar(3)'
where datatype = 'nvarchar(3)';

update indicator_metadata
set datatype = 'varchar(60)'
where datatype = 'nvarchar(60)';

update indicator_metadata
set datatype = 'varchar(40)'
where datatype = 'nvarchar(40)';

COPY domain_variable
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/domain_variable.csv' 
CSV delimiter '|';

COPY schema
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/schema.csv' 
CSV delimiter '|' encoding 'windows-1251';

-- Create SQL View for name, label and id
-- Create a table with id and shape as text, then run this update
update country 
set shape = (select ST_GeomFromText(placeholder.shape))
from placeholder
where placeholder.id = country.id;

COPY domain_country_results
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/domain_country_results.csv' 
CSV delimiter '|';

update GAUL_2012_2 set gaul_2012_2 = replace(GAUL_2012_2, 'Abuja', 'Abuja"');


/**
create view indicator_metadata_view
as
select id,
vi_id,
cast(	varCode	as varchar) as	varCode	,
cast(	varLabel	as varchar) as	varLabel	,
published,
cast(	tbName	as varchar) as	tbName	,
cast(	version	as varchar) as	version	,
cast(	tbKey	as varchar) as	tbKey	,
cast(	varTitle	as varchar) as	varTitle	,
cast(	varDesc	as varchar) as	varDesc	,
cast(	unit	as varchar) as	unit	,
dec,
cast(	year	as varchar) as	year	,
yearEnd,
cast(	cat1	as varchar) as	cat1	,
cast(	cat2	as varchar) as	cat2	,
cast(	cat3	as varchar) as	cat3	,
cast(	extent	as varchar) as	extent	,
cast(	type	as varchar) as	type	,
cast(	aggType	as varchar) as	aggType	,
cast(	aggFun	as varchar) as	aggFun	,
cast(	owner	as varchar) as	owner	,
cast(	caveat	as varchar) as	caveat	,
cast(	sources	as varchar) as	sources	,
cast(	sourceMini	as varchar) as	sourceMini	,
genRaster,
isDomain,
isProduct,
cast(	mxdName	as varchar) as	mxdName	,
cast(	classColors	as varchar) as	classColors	,
cast(	classBreaks	as varchar) as	classBreaks	,
cast(	classLabels	as varchar) as	classLabels	,
cast(	dataType	as varchar) as	dataType
from indicator_metadata
go  
**/

