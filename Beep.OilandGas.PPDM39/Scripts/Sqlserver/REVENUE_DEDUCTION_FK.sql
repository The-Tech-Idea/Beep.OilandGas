raiserror ('CREATING FOREIGN KEYS FOR REVENUE_DEDUCTION', 10,1) with nowait
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'revenue_transaction' AND type = 'U')
BEGIN
    ALTER TABLE revenue_deduction
    ADD CONSTRAINT revenue_deduction_REVENUE_TRANSACTION_FK FOREIGN KEY (REVENUE_TRANSACTION_ID) REFERENCES revenue_transaction(REVENUE_TRANSACTION_ID)
END

