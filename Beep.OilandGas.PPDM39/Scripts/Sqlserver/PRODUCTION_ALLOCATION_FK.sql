raiserror ('CREATING FOREIGN KEYS FOR PRODUCTION_ALLOCATION', 10,1) with nowait
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'pden' AND type = 'U')
BEGIN
    ALTER TABLE production_allocation
    ADD CONSTRAINT production_allocation_PDEN_FK FOREIGN KEY (PDEN_ID) REFERENCES pden(PDEN_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'field' AND type = 'U')
BEGIN
    ALTER TABLE production_allocation
    ADD CONSTRAINT production_allocation_FIELD_FK FOREIGN KEY (FIELD_ID) REFERENCES field(FIELD_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'well' AND type = 'U')
BEGIN
    ALTER TABLE production_allocation
    ADD CONSTRAINT production_allocation_WELL_FK FOREIGN KEY (WELL_ID) REFERENCES well(WELL_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'pool' AND type = 'U')
BEGIN
    ALTER TABLE production_allocation
    ADD CONSTRAINT production_allocation_POOL_FK FOREIGN KEY (POOL_ID) REFERENCES pool(POOL_ID)
END

