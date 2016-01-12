/*********************************************************************
	schef database schema postgres migration

		* Create Schema for HC_DB_WEB_2 in PostgreSQL

*********************************************************************/

CREATE TABLE indicator_metadata
(
  id serial NOT NULL,
  vi_id integer,
  varcode character varying,
  varlabel character varying,
  published bit(1),
  tbname character varying,
  version character varying,
  tbkey character varying,
  vartitle character varying,
  vardesc character varying,
  unit character varying,
  "dec" integer,
  year character varying,
  yearend integer,
  cat1 character varying,
  cat2 character varying,
  cat3 character varying,
  extent character varying,
  type character varying,
  aggtype character varying,
  aggfun character varying,
  owner character varying,
  caveat character varying,
  sources character varying,
  sourcemini character varying,
  genraster bit(1),
  isdomain bit(1),
  isproduct bit(1),
  mxdname character varying,
  classcolors character varying,
  classbreaks character varying,
  classlabels character varying,
  datatype character varying(25),
  CONSTRAINT indicator_metadata_pkey PRIMARY KEY (id)
);


-- Table: domain_variable

-- DROP TABLE domain_variable;

CREATE TABLE domain_variable
(
  id integer NOT NULL,
  column_name character varying(255),
  micro_label character varying(255),
  short_label character varying(255),
  full_label character varying(255),
  long_description character varying,
  category_1 character varying(255),
  category_2 character varying(255),
  category_3 character varying(255),
  item character varying(255),
  unit character varying(255),
  decimal_places integer,
  year integer,
  end_year integer,
  extent character varying(255),
  classification_type character varying(255),
  agg_type character varying(255),
  agg_formula character varying(255),
  table_name character varying(255),
  column_label character varying(255),
  table_unique_id character varying(255),
  source character varying(255),
  micro_source character varying(50),
  data_worker character varying(255),
  createdby character varying(255),
  createddate character varying(255),
  published bit(1),
  validated bit(1),
  last_processed character varying(255),
  processing_comment character varying(255),
  version character varying(50),
  caveat character varying,
  CONSTRAINT domain_variable_pkey PRIMARY KEY (id)
);

-- Table: classification

-- DROP TABLE classification;

CREATE TABLE classification
(
  id integer NOT NULL,
  name character varying(50),
  description character varying(255),
  createdby character varying(50),
  createddate timestamp without time zone,
  CONSTRAINT classification_pkey PRIMARY KEY (id)
);

-- Table: schema

-- DROP TABLE schema;

CREATE TABLE schema
(
  id integer NOT NULL,
  description character varying(255),
  name character varying(50) NOT NULL,
  preview_url character varying(50),
  createdby character varying(50),
  createddate timestamp without time zone,
  long_description character varying,
  published bit(1) NOT NULL,
  for_mappr bit(1),
  restrictions character varying(50),
  json_geometry character varying,
  CONSTRAINT schema_pkey PRIMARY KEY (id)
);


-- Table: collection

-- DROP TABLE collection;

CREATE TABLE collection
(
  name character varying(255),
  label character varying(255),
  code character varying(3) NOT NULL,
  CONSTRAINT collection_pkey PRIMARY KEY (code)
);

-- Table: collection_group

-- DROP TABLE collection_group;

CREATE TABLE collection_group
(
  id serial NOT NULL,
  "group" character varying(255) NOT NULL,
  collection_code character varying(3) NOT NULL,
  CONSTRAINT collection_group_pkey PRIMARY KEY (id),
  CONSTRAINT collection_group_collection_code_fkey FOREIGN KEY (collection_code)
      REFERENCES collection (code) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
);



-- Table: discrete_classification

-- DROP TABLE discrete_classification;

CREATE TABLE discrete_classification
(
  id integer NOT NULL,
  originalid character varying(50) NOT NULL,
  domainid integer NOT NULL,
  classid integer NOT NULL,
  sortorder integer,
  CONSTRAINT discrete_classification_pkey PRIMARY KEY (id),
  CONSTRAINT discrete_classification_classid_fkey FOREIGN KEY (classid)
      REFERENCES classification (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT discrete_classification_domainid_fkey FOREIGN KEY (domainid)
      REFERENCES domain_variable (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
);

-- Table: continuous_classification

-- DROP TABLE continuous_classification;

CREATE TABLE continuous_classification
(
  id integer NOT NULL,
  domainid integer NOT NULL,
  min double precision,
  max double precision,
  classid integer NOT NULL,
  sortorder integer,
  CONSTRAINT continuous_classification_pkey PRIMARY KEY (id),
  CONSTRAINT continuous_classification_classid_fkey FOREIGN KEY (classid)
      REFERENCES classification (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT continuous_classification_domainid_fkey FOREIGN KEY (domainid)
      REFERENCES domain_variable (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
);



-- Table: schema_domain

-- DROP TABLE schema_domain;

CREATE TABLE schema_domain
(
  id integer NOT NULL,
  schemaid integer NOT NULL,
  domainid integer NOT NULL,
  CONSTRAINT schema_domain_pkey PRIMARY KEY (id),
  CONSTRAINT schema_domain_domainid_fkey FOREIGN KEY (domainid)
      REFERENCES domain_variable (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT schema_domain_schemaid_fkey FOREIGN KEY (schemaid)
      REFERENCES schema (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
);

-- Table: country

-- DROP TABLE country;

CREATE TABLE country
(
  name character varying(10),
  label character varying(255),
  id character varying(3) NOT NULL,
  shape geometry,
  CONSTRAINT country_pkey PRIMARY KEY (id)
);


-- Table: country_collection

-- DROP TABLE country_collection;

CREATE TABLE country_collection
(
  id serial NOT NULL,
  country_id character varying(3) NOT NULL,
  collection_code character varying(3) NOT NULL,
  CONSTRAINT country_collection_pkey PRIMARY KEY (id),
  CONSTRAINT country_collection_collection_code_fkey FOREIGN KEY (collection_code)
      REFERENCES collection (code) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT country_collection_country_id_fkey FOREIGN KEY (country_id)
      REFERENCES country (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
);

-- Table: domain_country_results

-- DROP TABLE domain_country_results;

CREATE TABLE domain_country_results
(
  id serial NOT NULL,
  iso3 character varying(3) NOT NULL,
  schema_id integer NOT NULL,
  shape geometry,
  name character varying(255),
  CONSTRAINT domain_country_results_pkey PRIMARY KEY (id),
  CONSTRAINT domain_country_results_iso3_fkey FOREIGN KEY (iso3)
      REFERENCES country (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT domain_country_results_schema_id_fkey FOREIGN KEY (schema_id)
      REFERENCES schema (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
);

-- Table: gaul_2008_0

-- DROP TABLE gaul_2008_0;

CREATE TABLE gaul_2008_0
(
  cell5m integer NOT NULL,
  gaul_2008_0 character varying(100),
  CONSTRAINT gaul_2008_0_pkey PRIMARY KEY (cell5m)
);

-- Table: gaul_2008_1

-- DROP TABLE gaul_2008_1;

CREATE TABLE gaul_2008_1
(
  cell5m integer NOT NULL,
  gaul_2008_1 character varying(100),
  CONSTRAINT gaul_2008_1_pkey PRIMARY KEY (cell5m)
);

-- Table: gaul_2008_2

-- DROP TABLE gaul_2008_2;

CREATE TABLE gaul_2008_2
(
  cell5m integer NOT NULL,
  gaul_2008_2 character varying(100),
  CONSTRAINT gaul_2008_2_pkey PRIMARY KEY (cell5m)
);

-- Table: gaul_2012_0

-- DROP TABLE gaul_2012_0;

CREATE TABLE gaul_2012_0
(
  cell5m integer NOT NULL,
  gaul_2012_0 character varying(100),
  CONSTRAINT gaul_2012_0_pkey PRIMARY KEY (cell5m)
);

-- Table: gaul_2012_1

-- DROP TABLE gaul_2012_1;

CREATE TABLE gaul_2012_1
(
  cell5m integer NOT NULL,
  gaul_2012_1 character varying(100),
  CONSTRAINT gaul_2012_1_pkey PRIMARY KEY (cell5m)
);

-- Table: gaul_2012_2

-- DROP TABLE gaul_2012_2;

CREATE TABLE gaul_2012_2
(
  cell5m integer NOT NULL,
  gaul_2012_2 character varying(100),
  CONSTRAINT gaul_2012_2_pkey PRIMARY KEY (cell5m)
);

-- Table: toppr_area

-- DROP TABLE toppr_area;

CREATE TABLE toppr_area
(
  id serial NOT NULL,
  cell5m integer NOT NULL,
  iso3 character varying(3),
  gaul_2008_1 character varying(100),
  gaul_2012_1 character varying(100),
  area_crop numeric,
  vp_crop numeric,
  crop character varying(30),
  value numeric,
  CONSTRAINT toppr_area_pkey PRIMARY KEY (id)
);

-- Table: toppr_area2

-- DROP TABLE toppr_area2;

CREATE TABLE toppr_area2
(
  id serial NOT NULL,
  cell5m integer NOT NULL,
  iso3 character varying(3),
  gaul_2008_1 character varying(100),
  gaul_2012_1 character varying(100),
  area_crop numeric,
  vp_crop numeric,
  crop character varying(30),
  value numeric,
  CONSTRAINT toppr_area2_pkey PRIMARY KEY (id)
);

-- Table: toppr_prod

-- DROP TABLE toppr_prod;

CREATE TABLE toppr_prod
(
  id serial NOT NULL,
  cell5m integer NOT NULL,
  iso3 character varying(3),
  gaul_2008_1 character varying(100),
  gaul_2012_1 character varying(100),
  area_crop numeric,
  vp_crop numeric,
  crop character varying(30),
  value numeric,
  CONSTRAINT toppr_prod_pkey PRIMARY KEY (id)
);

-- Table: toppr_value

-- DROP TABLE toppr_value;

CREATE TABLE toppr_value
(
  id serial NOT NULL,
  cell5m integer NOT NULL,
  iso3 character varying(3),
  gaul_2008_1 character varying(100),
  gaul_2012_1 character varying(100),
  area_crop numeric,
  vp_crop numeric,
  crop character varying(30),
  value numeric,
  CONSTRAINT toppr_value_pkey PRIMARY KEY (id)
);

-- Table: msh_100k_id

-- DROP TABLE msh_100k_id;

CREATE TABLE msh_100k_id
(
  cell5m integer NOT NULL,
  msh_100k_id numeric,
  CONSTRAINT msh_100k_id_pkey PRIMARY KEY (cell5m)
);

-- Table: msh_20k_id

-- DROP TABLE msh_20k_id;

CREATE TABLE msh_20k_id
(
  cell5m integer NOT NULL,
  msh_20k_id numeric,
  CONSTRAINT msh_20k_id_pkey PRIMARY KEY (cell5m)
);

-- Table: msh_250k_id

-- DROP TABLE msh_250k_id;

CREATE TABLE msh_250k_id
(
  cell5m integer NOT NULL,
  msh_250k_id numeric,
  CONSTRAINT msh_250k_id_pkey PRIMARY KEY (cell5m)
);

-- Table: msh_500k_id

-- DROP TABLE msh_500k_id;

CREATE TABLE msh_500k_id
(
  cell5m integer NOT NULL,
  msh_500k_id numeric,
  CONSTRAINT msh_500k_id_pkey PRIMARY KEY (cell5m)
);

-- Table: msh_50k_id

-- DROP TABLE msh_50k_id;

CREATE TABLE msh_50k_id
(
  cell5m integer NOT NULL,
  msh_50k_id numeric,
  CONSTRAINT msh_50k_id_pkey PRIMARY KEY (cell5m)
);

-- Table: fs_code

-- DROP TABLE fs_code;

CREATE TABLE fs_code
(
  cell5m integer NOT NULL,
  fs_code numeric,
  CONSTRAINT fs_code_pkey PRIMARY KEY (cell5m)
);

-- Table: fs_name

-- DROP TABLE fs_name;

CREATE TABLE fs_name
(
  cell5m integer NOT NULL,
  fs_name character varying(50),
  CONSTRAINT fs_name_pkey PRIMARY KEY (cell5m)
);

-- Table: pd_ru00

-- DROP TABLE pd_ru00;

CREATE TABLE pd_ru00
(
  cell5m integer NOT NULL,
  pd_ru00 numeric,
  CONSTRAINT pd_ru00_pkey PRIMARY KEY (cell5m)
);

-- Table: pd_ru05

-- DROP TABLE pd_ru05;

CREATE TABLE pd_ru05
(
  cell5m integer NOT NULL,
  pd_ru05 numeric,
  CONSTRAINT pd_ru05_pkey PRIMARY KEY (cell5m)
);

-- Table: aez_code

-- DROP TABLE aez_code;

CREATE TABLE aez_code
(
  cell5m integer NOT NULL,
  aez_code numeric,
  CONSTRAINT aez_code_pkey PRIMARY KEY (cell5m)
);


-- Table: aez_text

-- DROP TABLE aez_text;

CREATE TABLE aez_text
(
  cell5m integer NOT NULL,
  aez_text character varying(50),
  CONSTRAINT aez_text_pkey PRIMARY KEY (cell5m)
);

-- Table: area_wbody

-- DROP TABLE area_wbody;

CREATE TABLE area_wbody
(
  cell5m integer NOT NULL,
  area_wbody numeric,
  CONSTRAINT area_wbody_pkey PRIMARY KEY (cell5m)
);

-- Table: cell_values

-- DROP TABLE cell_values;

CREATE TABLE cell_values
(
  id serial NOT NULL,
  cell5m integer NOT NULL,
  column_name character varying(30),
  value character varying(255),
  CONSTRAINT cell_values_pkey PRIMARY KEY (id)
);

-- Table: subnatunit

-- DROP TABLE subnatunit;

CREATE TABLE subnatunit
(
  cell5m integer NOT NULL,
  subnatunit character varying(255),
  CONSTRAINT subnatunit_pkey PRIMARY KEY (cell5m)
);

-- Table: etl_audit

-- DROP TABLE etl_audit;

CREATE TABLE etl_audit
(
  indicator character varying,
  row_count integer,
  created timestamp without time zone,
  updated timestamp without time zone
);



