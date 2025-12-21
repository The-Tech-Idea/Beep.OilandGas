raiserror ('CREATING FOREIGN KEYS FOR REVENUE_DISTRIBUTION', 10,1) with nowait
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'revenue_transaction' AND type = 'U')
BEGIN
    ALTER TABLE revenue_distribution
    ADD CONSTRAINT revenue_distribution_REVENUE_TRANSACTION_FK FOREIGN KEY (REVENUE_TRANSACTION_ID) REFERENCES revenue_transaction(REVENUE_TRANSACTION_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'business_associate' AND type = 'U')
BEGIN
    ALTER TABLE revenue_distribution
    ADD CONSTRAINT revenue_distribution_BUSINESS_ASSOCIATE_FK FOREIGN KEY (BUSINESS_ASSOCIATE_ID) REFERENCES business_associate(BA_ID)
END

