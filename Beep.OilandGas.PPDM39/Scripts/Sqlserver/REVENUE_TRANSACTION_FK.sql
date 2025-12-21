raiserror ('CREATING FOREIGN KEYS FOR REVENUE_TRANSACTION', 10,1) with nowait
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'finance' AND type = 'U')
BEGIN
    ALTER TABLE revenue_transaction
    ADD CONSTRAINT revenue_transaction_FINANCE_FK FOREIGN KEY (FINANCE_ID) REFERENCES finance(FINANCE_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'field' AND type = 'U')
BEGIN
    ALTER TABLE revenue_transaction
    ADD CONSTRAINT revenue_transaction_FIELD_FK FOREIGN KEY (FIELD_ID) REFERENCES field(FIELD_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'well' AND type = 'U')
BEGIN
    ALTER TABLE revenue_transaction
    ADD CONSTRAINT revenue_transaction_WELL_FK FOREIGN KEY (WELL_ID) REFERENCES well(WELL_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'pden' AND type = 'U')
BEGIN
    ALTER TABLE revenue_transaction
    ADD CONSTRAINT revenue_transaction_PDEN_FK FOREIGN KEY (PDEN_ID) REFERENCES pden(PDEN_ID)
END

