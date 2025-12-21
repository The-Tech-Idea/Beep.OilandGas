raiserror ('CREATING FOREIGN KEYS FOR ACCOUNTING_METHOD', 10,1) with nowait
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'field' AND type = 'U')
BEGIN
    ALTER TABLE accounting_method
    ADD CONSTRAINT accounting_method_FIELD_FK FOREIGN KEY (FIELD_ID) REFERENCES field(FIELD_ID)
END

