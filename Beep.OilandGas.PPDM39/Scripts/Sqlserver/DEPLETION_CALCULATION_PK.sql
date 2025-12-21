raiserror ('CREATING PRIMARY KEY FOR DEPLETION_CALCULATION', 10,1) with nowait
ALTER TABLE depletion_calculation
ADD CONSTRAINT depletion_calculation_PK PRIMARY KEY (DEPLETION_ID)

