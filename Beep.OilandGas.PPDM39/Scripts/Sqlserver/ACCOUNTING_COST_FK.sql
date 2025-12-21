raiserror ('CREATING FOREIGN KEYS FOR ACCOUNTING_COST', 10,1) with nowait
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'finance' AND type = 'U')
BEGIN
    ALTER TABLE accounting_cost
    ADD CONSTRAINT accounting_cost_FINANCE_FK FOREIGN KEY (FINANCE_ID) REFERENCES finance(FINANCE_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'field' AND type = 'U')
BEGIN
    ALTER TABLE accounting_cost
    ADD CONSTRAINT accounting_cost_FIELD_FK FOREIGN KEY (FIELD_ID) REFERENCES field(FIELD_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'well' AND type = 'U')
BEGIN
    ALTER TABLE accounting_cost
    ADD CONSTRAINT accounting_cost_WELL_FK FOREIGN KEY (WELL_ID) REFERENCES well(WELL_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'pool' AND type = 'U')
BEGIN
    ALTER TABLE accounting_cost
    ADD CONSTRAINT accounting_cost_POOL_FK FOREIGN KEY (POOL_ID) REFERENCES pool(POOL_ID)
END

