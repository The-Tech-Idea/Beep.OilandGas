raiserror ('CREATING FOREIGN KEYS FOR ASSET_RETIREMENT_OBLIGATION', 10,1) with nowait
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'field' AND type = 'U')
BEGIN
    ALTER TABLE asset_retirement_obligation
    ADD CONSTRAINT asset_retirement_obligation_FIELD_FK FOREIGN KEY (FIELD_ID) REFERENCES field(FIELD_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'well' AND type = 'U')
BEGIN
    ALTER TABLE asset_retirement_obligation
    ADD CONSTRAINT asset_retirement_obligation_WELL_FK FOREIGN KEY (WELL_ID) REFERENCES well(WELL_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'facility' AND type = 'U')
BEGIN
    ALTER TABLE asset_retirement_obligation
    ADD CONSTRAINT asset_retirement_obligation_FACILITY_FK FOREIGN KEY (FACILITY_ID) REFERENCES facility(FACILITY_ID)
END

