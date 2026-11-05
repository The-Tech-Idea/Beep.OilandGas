async (page) => {
 const observations=[];
 const reason='Signed site log reconciles this browser correction';
 const summary='Corrected observed operation from the signed browser site log';
 const observe=async label=>{
  const storage=await page.evaluate(({reason,summary})=>Object.keys(sessionStorage).filter(k=>k.endsWith('.drilling-report')).map(key=>({key,length:sessionStorage.getItem(key).length,hasPlainReason:sessionStorage.getItem(key).includes(reason),hasPlainSummary:sessionStorage.getItem(key).includes(summary)})),{reason,summary});
  observations.push({label,text:await page.locator('body').innerText(),metadata:await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json(),storage});
 };
 await page.request.post(new URL('/fixture/reports/seed/3',page.url()).href);
 await page.getByRole('button',{name:'Use editable mapped profile',exact:true}).click();
 await page.getByRole('button',{name:'Reload original report history',exact:true}).click();
 await page.getByRole('status').filter({hasText:'Showing history entries 1 to 3 of 3'}).waitFor({state:'visible'});
 await page.getByRole('button',{name:'Correct observations from entry 1',exact:true}).click();
 await page.getByRole('heading',{name:'Correct recorded observations',exact:true}).waitFor({state:'visible'});
 await page.getByRole('combobox',{name:'Current report recorder role',exact:true}).click();
 await page.getByRole('option').click(); await observe('prefilled-earlier-period');
 await page.getByRole('textbox',{name:'Correction reason*',exact:true}).fill(reason);
 await page.getByRole('button',{name:'Review observed drilling report',exact:true}).click();
 await page.getByRole('alert').filter({hasText:'Confirm the actual period'}).waitFor({state:'visible'}); await observe('unchanged-values-refused');
 await page.getByRole('textbox',{name:'Observed operations summary*',exact:true}).fill(summary);
 await page.getByRole('button',{name:'Review observed drilling report',exact:true}).click();
 await page.getByRole('heading',{name:'Confirm original observed report',exact:true}).waitFor({state:'visible'}); await observe('frozen-correction');
 await page.getByRole('button',{name:'Confirm and retain observed report',exact:true}).click();
 await page.waitForFunction(()=>[...document.querySelectorAll('button')].some(b=>b.textContent.trim()==='Recover original drilling report'&&!b.disabled)); await observe('lost-ack-retained');
 await page.getByRole('button',{name:'Use read-only unmapped profile',exact:true}).click();
 await page.getByRole('button',{name:'Reload original report history',exact:true}).click();
 await page.waitForFunction(()=>[...document.querySelectorAll('button')].some(b=>b.textContent.trim()==='Recover original drilling report'&&!b.disabled)); await observe('readonly-retained-reload');
 const recover=page.getByRole('button',{name:'Recover original drilling report',exact:true}); await recover.focus(); await page.keyboard.press('Enter');
 await page.getByRole('heading',{name:'Original report outcome',exact:true}).waitFor({state:'visible'}); await observe('readonly-keyboard-recovered');
 await page.getByRole('button',{name:'Reload original report history',exact:true}).click();
 await page.getByRole('status').filter({hasText:'Showing history entries 1 to 4 of 4'}).waitFor({state:'visible'}); await observe('current-superseded-readonly-history');
 for(const [label,name] of [['other-company-denied','Switch other company'],['other-account-denied','Switch other account']]){
  await page.getByRole('button',{name,exact:true}).click(); await page.getByRole('button',{name:'Reload original report history',exact:true}).click();
  await page.getByRole('alert').filter({hasText:'The original execution/report history could not be confirmed'}).waitFor({state:'visible'}); await observe(label);
 }
 await page.getByRole('button',{name:'Restore original company/account',exact:true}).click(); await page.getByRole('button',{name:'Reload original report history',exact:true}).click();
 await page.getByRole('status').filter({hasText:'Showing history entries 1 to 4 of 4'}).waitFor({state:'visible'}); await observe('restored-readonly-history');
 return {reason,summary,observations};
}
