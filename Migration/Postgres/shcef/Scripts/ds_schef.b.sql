/*********************************************************************
  schef data postgres migration

    * Load data from schef to postgresql

*********************************************************************/

--- exec sp_columns [tablename]
/**
select cell5m, NAME_ADM1 from cell5m_spam2005v1_group_valprod
select cell5m, ISO3 from cell5m_spam2005v1_group_valprod
select cell5m, PROD_LEVEL from cell5m_spam2005v1_group_valprod
select cell5m, ALLOC_KEY from cell5m_spam2005v1_group_valprod
select cell5m, REC_TYPE from cell5m_spam2005v1_group_valprod
select cell5m, UNIT from cell5m_spam2005v1_group_valprod
select cell5m, CREA_DATE from cell5m_spam2005v1_group_valprod
select cell5m, YEAR_DATA from cell5m_spam2005v1_group_valprod
select cell5m, SOURCE from cell5m_spam2005v1_group_valprod
select cell5m, SCALE_YEAR from cell5m_spam2005v1_group_valprod
select cell5m, NAME_CNTR from cell5m_spam2005v1_group_valprod

select count( ISO3 ) from  cell5m_spam2005v1_valprod
select count( PROD_LEVEL ) from  cell5m_spam2005v1_valprod
select count( ALLOC_KEY ) from  cell5m_spam2005v1_valprod
select count( REC_TYPE ) from  cell5m_spam2005v1_valprod
select count( UNIT ) from  cell5m_spam2005v1_valprod
select count( CREA_DATE ) from  cell5m_spam2005v1_valprod
select count( YEAR_DATA ) from  cell5m_spam2005v1_valprod
select count( SOURCE ) from  cell5m_spam2005v1_valprod
select count( SCALE_YEAR ) from  cell5m_spam2005v1_valprod
select count( NAME_CNTR ) from  cell5m_spam2005v1_valprod
select count( NAME_ADM1 ) from  cell5m_spam2005v1_valprod

=IF(TEXT(A1, "#########.#")=TEXT(B1,"#########.#"),TRUE,FALSE)




2007,2010
,,Ghana ?
Congo, Rep
Congo, Dem R
**/

-- Export data from sql server schef to postgres schef



COPY cell5m_bio 
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_bio.csv' 
DELIMITER ',' CSV;

COPY cell5m_fcc
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_fcc.csv' 
DELIMITER ',' CSV;

COPY cell5m_fs
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_fs.csv' 
CSV;
-- update csv and change the last column so that commas are not periods
-- fix the comma issue on the last attribute
UPDATE cell5m_fs SET sub_syst = replace(sub_syst, '.', ',');

COPY cell5m_pest
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_pest.csv' 
CSV;

COPY cell5m_pop_00
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_pop_00.csv' 
CSV;

COPY cell5m_pop_05
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_pop_05.csv' 
CSV;

COPY cell5m_pov_05
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_pov_05.csv' 
CSV;

-- manually fix the seperates of dates and commas in last attriubute 
COPY cell5m_spam2005v1_group_harvestarea
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_spam2005v1_group_harvestarea.csv' 
CSV;

-- replace the periods that were just manually replaced
UPDATE cell5m_spam2005v1_group_harvestarea SET name_cntr = replace(name_cntr, '.', ',');
UPDATE cell5m_spam2005v1_group_harvestarea SET year_data = replace(year_data, '.', ',');


-- manually fix the seperates of dates and commas in last attriubute 
COPY cell5m_spam2005v1_group_production
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_spam2005v1_group_production.csv' 
CSV;

-- replace the periods that were just manually replaced
UPDATE cell5m_spam2005v1_group_production SET name_cntr = replace(name_cntr, '.', ',');
UPDATE cell5m_spam2005v1_group_production SET year_data = replace(year_data, '.', ',');

-- manually fix the seperates of dates and commas in last attriubute 
COPY cell5m_spam2005v1_group_valprod
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_spam2005v1_group_valprod.csv' 
CSV;

-- replace the periods that were just manually replaced
UPDATE cell5m_spam2005v1_group_valprod SET name_cntr = replace(name_cntr, '.', ',');
UPDATE cell5m_spam2005v1_group_valprod SET year_data = replace(year_data, '.', ',');

-- manually fix the seperates of dates and commas in last attriubute 
COPY cell5m_spam2005v1_harvestarea
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_spam2005v1_harvestarea.csv' 
CSV;

-- replace the periods that were just manually replaced
UPDATE cell5m_spam2005v1_harvestarea SET name_cntr = replace(name_cntr, '.', ',');
UPDATE cell5m_spam2005v1_harvestarea SET year_data = replace(year_data, '.', ',');

-- manually fix the seperates of dates and commas in last attriubute 
COPY cell5m_spam2005v1_production 
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_spam2005v1_production.csv' 
CSV; 
-- replace the periods that were just manually replaced
UPDATE cell5m_spam2005v1_production SET name_cntr = replace(name_cntr, '.', ',');
UPDATE cell5m_spam2005v1_production SET year_data = replace(year_data, '.', ',');

-- manually fix the seperates of dates and commas in last attriubute 
COPY cell5m_spam2005v1_valprodagg 
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_spam2005v1_valprodagg.csv' 
CSV; 
-- replace the periods that were just manually replaced
UPDATE cell5m_spam2005v1_valprodagg SET name_cntr = replace(name_cntr, '.', ',');
UPDATE cell5m_spam2005v1_valprodagg SET year_data = replace(year_data, '.', ',');

-- manually fix the seperates of dates and commas in last attriubute 
COPY cell5m_spam2005v1_yield
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_spam2005v1_yield.csv' 
CSV; 
-- replace the periods that were just manually replaced
UPDATE cell5m_spam2005v1_yield SET name_cntr = replace(name_cntr, '.', ',');
UPDATE cell5m_spam2005v1_yield SET year_data = replace(year_data, '.', ',');

--  
COPY cell5m_tt
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_tt.csv' 
CSV;
--
COPY cell5m_wdpa2009
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_wdpa2009.csv' 
CSV;


COPY cell5m_dhs 
FROM '/Users/SpatialDev/Library/Application Support/Postgres/var-9.3/cell5m_dhs.csv' 
DELIMITER ',' CSV;


-- Update special characters
update cell5m_dhs set svyl1nm = replace(svyl1nm, 'Ô', 'ï');
update cell5m_dhs set svyl1nm = replace(svyl1nm, 'È', 'é');
update cell5m_dhs set svyl1nm = replace(svyl1nm, 'Ë', 'è');
update cell5m_dhs set svyl1nm = replace(svyl1nm, 'Í', 'ê');
update cell5m_dhs set svyl1nm = replace(svyl1nm, '„', 'ã');
update cell5m_dhs set svyl1nm = 'regico do principe' where cell5m = 4581448 and svyl1nm = 'região do principe';

/**
-- Create view for tables with nvarchar
CREATE VIEW cell5m_bio_view
  AS 
  select cell5m, CAST([AEZ16_CLAS]AS varchar) as AEZ16_CLAS, AEZ16_CODE, CAST([AEZ5_CLAS] AS varchar) as AEZ5_CLAS, CAST([AEZ8_CLAS]AS varchar) as AEZ8_CLAS, ELEVATION, LGP_AVG, LGP_CV, CAST([LGP_TEXT]AS varchar) as LGP_TEXT, CAST([MAJ_NAME]AS varchar) as MAJ_NAME, PRE_AVG, PRE_CV from 
  dbo.cell5m_bio
  GO

CREATE VIEW cell5m_fs_view
  AS 
  select OBJECTID_1, FID_1, cast(alloc_key as varchar) as alloc_key, CELL5M, cast(FIPS0 as varchar) as FIPS0,
cast(ISO3_CNTR as varchar) as ISO3_CNTR, GAUL_CNTR, cast(GAUL_NAME as varchar) as GAUL_NAME, FS_2012, 
cast(FS_2012_TX as varchar) as FS_2012_TX, LEV_2, cast(SUB_SYST as varchar) as SUB_SYST
from dbo.cell5m_fs
  GO

  CREATE VIEW cell5m_pest_view
  AS 
  select cell5m, RF_GI, RF_EI, cast(RF_GI_CLAS as varchar) as RF_GI_CLASS, cast(RF_EI_CLAS as varchar) as RF_EI_CLASS,
  IR_GI, IR_EI, cast(IR_GI_CLAS as varchar) as IR_GI_CLASS, cast(IR_EI_CLAS as varchar) as IR_EI_CLASS
  FROM cell5m_pest
  GO

  CREATE VIEW cell5m_pop_00_view
  AS
  SELECT cell5m, RPOV_NR075, RPOV_NR125, RPOV_NR200, TPOV_NR075, TPOV_NR125, TPOV_NR200, TPOV_PT075,
  PD00_RUR, PN00_RUR, PN00_TOT, PN00_URB, cast(svy_pov as varchar) as SVY_POV, cast(svy_pop as varchar) as SVY_POP
  FROM cell5m_pop_00
  GO


CREATE VIEW cell5m_dhs_view
  AS 
  select CELL5M,
cast(svyCode as varchar) as svyCode,
cast(svyL1Nm as varchar) as svyL1Nm,
m4_duration,
m5_duration,
mother_age,
mother_weight_kgs,
mother_height_cms,
bmi,
child_mortality,
infant_mortality,
vitamin_a,
iron,
stunted_low,
stunted_moderate,
stunted_severe,
wasted_low,
wasted_moderate,
wasted_severe,
underweight_low,
underweight_moderate,
underweight_severe,
bmi_normal,
bmi_normal_adj,
bmi_obese,
bmi_obese_adj,
bmi_overweight,
bmi_overweight_adj,
bmi_underweight,
bmi_underweight_adj,
diarrhea,
wealth,
m4_duration_rur,
m5_duration_rur,
mother_age_rur,
mother_weight_kgs_rur,
mother_height_cms_rur,
bmi_rur,
child_mortality_rur,
infant_mortality_rur,
vitamin_a_rur,
iron_rur,
stunted_low_rur,
stunted_moderate_rur,
stunted_severe_rur,
wasted_low_rur,
wasted_moderate_rur,
wasted_severe_rur,
underweight_low_rur,
underweight_moderate_rur,
underweight_severe_rur,
bmi_normal_rur,
bmi_normal_adj_rur,
bmi_obese_rur,
bmi_obese_adj_rur,
bmi_overweight_rur,
bmi_overweight_adj_rur,
bmi_underweight_rur,
bmi_underweight_adj_rur,
diarrhea_rur,
wealth_rur,
m4_duration_urb,
m5_duration_urb,
mother_age_urb,
mother_weight_kgs_urb,
mother_height_cms_urb,
bmi_urb,
child_mortality_urb,
infant_mortality_urb,
vitamin_a_urb,
iron_urb,
stunted_low_urb,
stunted_moderate_urb,
stunted_severe_urb,
wasted_low_urb,
wasted_moderate_urb,
wasted_severe_urb,
underweight_low_urb,
underweight_moderate_urb,
underweight_severe_urb,
bmi_normal_urb,
bmi_normal_adj_urb,
bmi_obese_urb,
bmi_obese_adj_urb,
bmi_overweight_urb,
bmi_overweight_adj_urb,
bmi_underweight_urb,
bmi_underweight_adj_urb,
diarrhea_urb,
wealth_urb,
weight_rur,
weight_urb,
weight
from dbo.cell5m_dhs
where cell5m = 4309229
  GO


  create view cell5m_pov_05_view
as
select CELL5M,
cast(ISO3 as varchar) as ISO3,
cell5m_pov_05.year,
cast(svyCode as varchar) as svyCode,
svyL1Cd,
cast(svyL1Nm as varchar) as svyL1Nm,
cast(prttyNm as varchar) as prttyNm,
UPOV_PT125,
RPOV_PT125,
UPOV_PT200,
RPOV_PT200,
UPOV_GAP125,
RPOV_GAP125,
UPOV_GAP200,
RPOV_GAP200,
UPOV_SEV125,
RPOV_SEV125,
UPOV_SEV200,
RPOV_SEV200,
UPOV_SD125,
RPOV_SD125,
UPOV_SD200,
RPOV_SD200,
UPOV_PCEXP,
RPOV_PCEXP,
UPOV_GINI,
RPOV_GINI,
TPOV_PT125,
TPOV_PT200,
TPOV_GAP125,
TPOV_GAP200,
TPOV_SEV125,
TPOV_SEV200,
TPOV_SD125,
TPOV_SD200,
TPOV_PCEXP,
TPOV_GINI,
UPOV_PD125,
RPOV_PD125,
TPOV_PD125,
UPOV_PD200,
RPOV_PD200,
TPOV_PD200
from cell5m_pov_05
go


**/
