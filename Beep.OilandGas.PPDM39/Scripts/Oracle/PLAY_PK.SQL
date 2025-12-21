-- PPDM39 Oracle DDL Script
-- Converted from SQL Server script: PLAY_PK.sql
-- This script is for the PPDM39 model

PROMPT CREATING PRIMARY KEY PLAY_PK with nowait
ALTER TABLE PLAY ADD CONSTRAINT PLAY_PK PRIMARY KEY
(
	PLAY_ID
);

