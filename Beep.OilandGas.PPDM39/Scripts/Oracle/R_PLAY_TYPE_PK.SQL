-- PPDM39 Oracle DDL Script
-- Converted from SQL Server script: R_PLAY_TYPE_PK.sql
-- This script is for the PPDM39 model

PROMPT CREATING PRIMARY KEY R_PLAY_TYPE_PK with nowait
ALTER TABLE R_PLAY_TYPE ADD CONSTRAINT R_PLAY_TYPE_PK PRIMARY KEY
(
	PLAY_TYPE
);
