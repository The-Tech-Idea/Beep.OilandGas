@echo off

:: WARNING
:: Use of this data is subject to the Terms and Conditions outlined in the file PPDM_TermsAndConditions.txt
:: By opening and accessing this information, you are agreeing to these Terms and Conditions.
::
::
::  This SQL Server 2012 DDL is for the PPDM39 model
::  This is the BETA version


:: CONFIGURATION

:: Set to the name of your database instance
set database_name=PPDM39

:: Set to 1 to drop any existing database found with the same name
set perform_drop=1

:: Set to 1 to create a new database instance
set perform_create=1



:: DRAGONS

:: Temp script names
set create_script=CREATE_DB.sql
set drop_script=DROP_DB.sql
set sqlcmd=sqlcmd
set ver=PPDM 3.9


if %perform_drop%==1 (
	echo Dropping DB [%database_name%]
	echo DROP DATABASE %database_name% > %drop_script%
	echo GO >> %drop_script%
	%sqlcmd% -i %drop_script%
)

if %perform_create%==1 (
	echo Creating DB [%database_name%]
	echo CREATE DATABASE %database_name% > %create_script%
	echo GO >> %create_script%
	%sqlcmd% -i %create_script%
)

echo Creating Tables and Columns [TAB.sql]
echo This script is mandatory for %ver%
%sqlcmd% -d %database_name% -i TAB.sql


echo Creating Primary Keys [PK.sql]
echo This script is mandatory for %ver%
%sqlcmd% -d %database_name% -i PK.sql


echo Creating Check Constraints [CK.sql]
echo This script is mandatory for %ver%
%sqlcmd% -d %database_name% -i CK.sql 


echo Creating Foreign Key Constraints [FK.sql]
echo This script is mandatory for %ver%
%sqlcmd% -d %database_name% -i FK.sql 


echo Creating Original Units of Measture Foreign Keys [OUOM.sql]
echo This script is mandatory for %ver%
%sqlcmd% -d %database_name% -i OUOM.sql 


echo Creating Units of Measure Foreign Keys [UOM.sql]
echo This script is mandatory for %ver%
%sqlcmd% -d %database_name% -i UOM.sql 


echo Creating ROW_QUALITY Foreign Keys [RQUAL.sql]
echo This script is mandatory for %ver%
%sqlcmd% -d %database_name% -i RQUAL.sql 


echo Creating SOURCE Foreign Keys [RSRC.sql]
echo This script is mandatory for %ver%
%sqlcmd% -d %database_name% -i RSRC.sql 


echo Creating table comments [TCM.sql]
echo This script is OPTIONAL for %ver%, but is strongly recommended
%sqlcmd% -d %database_name% -i TCM.sql 


echo Creating column comments [CCM.sql]
echo This script is OPTIONAL for %ver%, but is strongly recommended
%sqlcmd% -d %database_name% -i CCM.sql 


echo Creating Synonyms [SYN.sql]
echo This script is OPTIONAL for %ver%, but is strongly recommended
%sqlcmd% -d %database_name% -i SYN.sql 


echo Creating Unique constraints and Not Null constraints on PPDM_GUID [GUID.sql]
echo This script is OPTIONAL for %ver%
%sqlcmd% -d %database_name% -i GUID.sql 


echo Cleaning up temp files..
if exist %create_script% del %create_script%
if exist %drop_script% del %drop_script%

