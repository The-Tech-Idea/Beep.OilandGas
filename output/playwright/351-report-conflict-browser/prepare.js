async (page) => {
 const m=await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json();
 await page.getByRole('button',{name:'Inspect current report basis',exact:true}).click();
 await page.getByRole('combobox',{name:'Current report recorder role',exact:true}).click();
 await page.getByRole('option').filter({hasText:m.role}).click();
 await page.getByRole('textbox',{name:'Observed period start UTC*',exact:true}).fill(m.periodStart);
 await page.getByRole('textbox',{name:'Observed period end UTC*',exact:true}).fill(m.periodEnd);
 await page.getByRole('spinbutton',{name:'Observed hole MD at start*',exact:true}).fill('100');
 await page.getByRole('spinbutton',{name:'Observed hole MD at end*',exact:true}).fill('150');
 await page.getByRole('combobox',{name:'Observed MD unit',exact:true}).click();
 await page.getByRole('option').filter({hasText:m.unit}).click();
 await page.getByRole('textbox',{name:'Report source provenance*',exact:true}).fill('Recorded browser site log 472');
 await page.getByRole('textbox',{name:'Observed operations summary*',exact:true}).fill('Recorded actual observations from site log');
 await page.getByRole('button',{name:'Review observed drilling report',exact:true}).click();
 await page.getByRole('heading',{name:'Confirm original observed report',exact:true}).waitFor({state:'visible'});
 return {text:await page.locator('body').innerText()};
}