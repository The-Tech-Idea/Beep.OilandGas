raiserror ('CREATING FOREIGN KEYS FOR ROYALTY_CALCULATION', 10,1) with nowait
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'obligation' AND type = 'U')
BEGIN
    ALTER TABLE royalty_calculation
    ADD CONSTRAINT royalty_calculation_OBLIGATION_FK FOREIGN KEY (OBLIGATION_ID) REFERENCES obligation(OBLIGATION_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'revenue_transaction' AND type = 'U')
BEGIN
    ALTER TABLE royalty_calculation
    ADD CONSTRAINT royalty_calculation_REVENUE_TRANSACTION_FK FOREIGN KEY (REVENUE_TRANSACTION_ID) REFERENCES revenue_transaction(REVENUE_TRANSACTION_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'field' AND type = 'U')
BEGIN
    ALTER TABLE royalty_calculation
    ADD CONSTRAINT royalty_calculation_FIELD_FK FOREIGN KEY (FIELD_ID) REFERENCES field(FIELD_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'well' AND type = 'U')
BEGIN
    ALTER TABLE royalty_calculation
    ADD CONSTRAINT royalty_calculation_WELL_FK FOREIGN KEY (WELL_ID) REFERENCES well(WELL_ID)
END

