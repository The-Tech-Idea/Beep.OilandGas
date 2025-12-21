raiserror ('CREATING FOREIGN KEYS FOR JIB_COST_ALLOCATION', 10,1) with nowait
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'joint_interest_bill' AND type = 'U')
BEGIN
    ALTER TABLE jib_cost_allocation
    ADD CONSTRAINT jib_cost_allocation_JIB_FK FOREIGN KEY (JIB_ID) REFERENCES joint_interest_bill(JIB_ID)
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'finance' AND type = 'U')
BEGIN
    ALTER TABLE jib_cost_allocation
    ADD CONSTRAINT jib_cost_allocation_FINANCE_FK FOREIGN KEY (FINANCE_ID) REFERENCES finance(FINANCE_ID)
END

