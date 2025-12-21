raiserror ('CREATING PRIMARY KEY FOR JOINT_INTEREST_BILL', 10,1) with nowait
ALTER TABLE joint_interest_bill
ADD CONSTRAINT joint_interest_bill_PK PRIMARY KEY (JIB_ID)

