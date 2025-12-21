-- PPDM39 Oracle DDL Script
-- Converted from SQL Server script: R_TRAP_TYPE_PK.sql
-- This script is for the PPDM39 model

PROMPT CREATING PRIMARY KEY R_TRAP_TYPE_PK with nowait
ALTER TABLE R_TRAP_TYPE ADD CONSTRAINT R_TRAP_TYPE_PK PRIMARY KEY
(
	TRAP_TYPE
);
