async (page) => {
  await page.getByRole("button", {name:"Show actual reporting assignment",exact:true}).click();
  await page.getByRole("button", {name:"Reload source and recipients",exact:true}).click();
  await page.getByRole("combobox", {name:"Configured reporting workflow",exact:true}).click();
  await page.getByRole("option").filter({hasText:"Configured ongoing execution reporting"}).click();
  await page.getByRole("combobox", {name:"Your assignment sender role",exact:true}).click();
  await page.getByRole("option").filter({hasText:"ConfiguredExecutionAssigner"}).click();
  await page.getByRole("textbox", {name:"Assignment reason*",exact:true}).fill("Frozen actual reporting destination 473");
  await page.getByRole("button", {name:"Review reporting assignment",exact:true}).click();
  await page.getByRole("heading",{name:"Confirm original reporting assignment",exact:true}).waitFor({state:"visible"});
  return {frozen:await page.getByRole("heading",{name:"Confirm original reporting assignment",exact:true}).isVisible()};
}