raiserror ('CREATING FOREIGN KEYS FOR DEPLETION_CALCULATION', 10,1) with nowait
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'field' AND type = 'U')
BEGIN
    ALTER TABLE depletion_calculation
    ADD CONSTRAINT depletion_calculation_FIELD_FK FOREIGN KEY (FIELD_ID) REFERENCES field(FIELD_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'well' AND type = 'U')
BEGIN
    ALTER TABLE depletion_calculation
    ADD CONSTRAINT depletion_calculation_WELL_FK FOREIGN KEY (WELL_ID) REFERENCES well(WELL_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'pool' AND type = 'U')
BEGIN
    ALTER TABLE depletion_calculation
    ADD CONSTRAINT depletion_calculation_POOL_FK FOREIGN KEY (POOL_ID) REFERENCES pool(POOL_ID)
END

