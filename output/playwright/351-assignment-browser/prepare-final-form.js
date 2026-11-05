async (page) => {
  await page.getByRole("button", {name:"Show actual reporting assignment",exact:true}).click();
  await page.getByRole("combobox", {name:"Configured reporting workflow",exact:true}).click();
  await page.getByRole("option").filter({hasText:"Configured ongoing execution reporting"}).click();
  await page.getByRole("combobox", {name:"Your assignment sender role",exact:true}).click();
  await page.getByRole("option").filter({hasText:"ConfiguredExecutionAssigner"}).click();
  await page.getByRole("textbox", {name:"Assignment reason*",exact:true}).fill("Quota original on corrected reporting screen 472");
  await page.getByRole("button", {name:"Review reporting assignment",exact:true}).click();
  return {frozen:await page.getByRole("heading",{name:"Confirm original reporting assignment",exact:true}).isVisible()};
}