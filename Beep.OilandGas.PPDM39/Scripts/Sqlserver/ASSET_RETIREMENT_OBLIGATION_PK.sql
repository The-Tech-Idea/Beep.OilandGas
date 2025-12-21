raiserror ('CREATING PRIMARY KEY FOR ASSET_RETIREMENT_OBLIGATION', 10,1) with nowait
ALTER TABLE asset_retirement_obligation
ADD CONSTRAINT asset_retirement_obligation_PK PRIMARY KEY (ARO_ID)

