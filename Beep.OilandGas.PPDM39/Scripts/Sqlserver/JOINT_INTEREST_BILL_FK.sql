raiserror ('CREATING FOREIGN KEYS FOR JOINT_INTEREST_BILL', 10,1) with nowait
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'interest_set' AND type = 'U')
BEGIN
    ALTER TABLE joint_interest_bill
    ADD CONSTRAINT joint_interest_bill_INTEREST_SET_FK FOREIGN KEY (INTEREST_SET_ID) REFERENCES interest_set(INT_SET_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'field' AND type = 'U')
BEGIN
    ALTER TABLE joint_interest_bill
    ADD CONSTRAINT joint_interest_bill_FIELD_FK FOREIGN KEY (FIELD_ID) REFERENCES field(FIELD_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'business_associate' AND type = 'U')
BEGIN
    ALTER TABLE joint_interest_bill
    ADD CONSTRAINT joint_interest_bill_OPERATOR_FK FOREIGN KEY (OPERATOR_ID) REFERENCES business_associate(BA_ID)
END

