
-- Function: etl_cellvalues()

-- DROP FUNCTION etl_cellvalues();

CREATE OR REPLACE FUNCTION etl_cellvalues()
  RETURNS void AS
$BODY$
DECLARE 
  sqlcmd  character varying;
  id integer;
  message character varying;
  columnName character varying;
  tableName character varying;
  dataType character varying;
  cursor1 CURSOR FOR 
    SELECT im.id, lower(im.varCode), im.tbName, im.dataType
  FROM indicator_metadata im
  WHERE published is true 
  AND varCode NOT IN ('CELL5M', 'CELL_VALUES', 'CELL_VALUES_old', 
    'classification', 'collection', 'collection_group', 'continuous_classification',
    'country','country_collection','discrete_classification','domain_country_results',
    'domain_variable','drupal_metadata','indicator_metadata','schema','schema_domain',
    'Variable_Inventory');
BEGIN 
-- FETCH next FROM cursor1 INTO id, columnName, tableName, dataType;

OPEN cursor1;
LOOP

FETCH next FROM cursor1 INTO id, columnName, tableName, dataType;
EXIT WHEN NOT FOUND;

if id is not null then 
RAISE NOTICE 'Processing indicator table number: % ----> % ', to_char(id,'999999999999'),columnName;
end if;

-- drop if table exists
IF ((SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE' 
AND table_schema = 'public' AND table_name = columnName) IS NOT NULL) THEN
  EXECUTE 'DROP TABLE ' || columnName;
END IF;

-- update the ETL Audit Table
IF ((SELECT indicator FROM ETL_Audit where indicator = lower(columnName)) IS NULL and columnName is not null) THEN
  INSERT INTO ETL_Audit (indicator,created,updated) VALUES(columnName ,LOCALTIMESTAMP, LOCALTIMESTAMP);
  RAISE NOTICE 'Inserted % into ETL_Audit Table', columnName; 
END IF;

if columnName is not null then
-- create table
EXECUTE 'CREATE TABLE ' || columnName || '(cell5m integer NOT NULL PRIMARY KEY,'|| columnName || ' ' || dataType || ')';
--RAISE EXCEPTION 'The table % cannot be created', columnName;
RAISE NOTICE 'Created table %', columnName;
end if;

-- load table
EXECUTE 'INSERT INTO ' || columnName || ' SELECT cell5m, ' || columnName || ' from ' || tableName;

-- create index
IF (dataType NOT LIKE '%char%') THEN
  EXECUTE 'CREATE INDEX IDX_' || columnName || ' on ' || columnName || '(' || columnName || ' ASC)';
END IF;

END LOOP;
CLOSE cursor1;

END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION etl_cellvalues()
  OWNER TO postgres;

