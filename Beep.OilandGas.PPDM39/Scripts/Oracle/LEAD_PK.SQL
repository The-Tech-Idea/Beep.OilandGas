-- PPDM39 Oracle DDL Script
-- Converted from SQL Server script: LEAD_PK.sql
-- This script is for the PPDM39 model

PROMPT CREATING PRIMARY KEY LEAD_PK with nowait
ALTER TABLE LEAD ADD CONSTRAINT LEAD_PK PRIMARY KEY
(
	LEAD_ID
);

