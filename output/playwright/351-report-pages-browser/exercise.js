async (page) => {
 const observations=[]; const request=async path=>page.request.post(new URL(path,page.url()).href);
 const observe=async label=>{observations.push({label,text:await page.locator('body').innerText(),metadata:await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json(),storedOriginals:await page.evaluate(()=>Object.keys(sessionStorage).filter(k=>k.endsWith('.drilling-report')))});};
 await request('/fixture/reports/seed/53');
 await page.getByRole('button',{name:'Use read-only unmapped profile',exact:true}).click();
 await page.getByRole('button',{name:'Reload original report history',exact:true}).click();
 await page.getByRole('status').filter({hasText:'Showing reports 1 to 25 of 53'}).waitFor({state:'visible',timeout:30000}); await observe('first-page');
 const next=page.getByRole('button',{name:'Next reports',exact:true}); await next.focus(); await page.keyboard.press('Enter');
 await page.getByRole('status').filter({hasText:'Showing reports 26 to 50 of 53'}).waitFor({state:'visible',timeout:30000}); await observe('keyboard-next-page');
 await next.click(); await page.getByRole('status').filter({hasText:'Showing reports 51 to 53 of 53'}).waitFor({state:'visible',timeout:30000}); await observe('last-page');
 const disabledLast=await next.isDisabled();
 await page.getByRole('button',{name:'Previous reports',exact:true}).click();
 await page.getByRole('status').filter({hasText:'Showing reports 26 to 50 of 53'}).waitFor({state:'visible',timeout:30000});
 await page.getByRole('button',{name:'Previous reports',exact:true}).click();
 await page.getByRole('status').filter({hasText:'Showing reports 1 to 25 of 53'}).waitFor({state:'visible',timeout:30000}); await observe('previous-first-page');
 await request('/fixture/reports/append'); await next.click();
 await page.getByRole('alert').filter({hasText:'The checked report history or current access is unconfirmed'}).waitFor({state:'visible',timeout:30000}); await observe('changed-history-refused');
 await page.getByRole('button',{name:'Reload original report history',exact:true}).click();
 await page.getByRole('status').filter({hasText:'Showing reports 1 to 25 of 54'}).waitFor({state:'visible',timeout:30000}); await observe('explicit-current-reload');
 for (const [label,name] of [['other-company-denied','Switch other company'],['other-account-denied','Switch other account']]) {
  await page.getByRole('button',{name,exact:true}).click(); await page.getByRole('button',{name:'Reload original report history',exact:true}).click();
  await page.getByRole('alert').filter({hasText:'The original execution/report history could not be confirmed'}).waitFor({state:'visible',timeout:30000}); await observe(label);
 }
 await page.getByRole('button',{name:'Restore original company/account',exact:true}).click(); await page.getByRole('button',{name:'Reload original report history',exact:true}).click();
 await page.getByRole('status').filter({hasText:'Showing reports 1 to 25 of 54'}).waitFor({state:'visible',timeout:30000}); await observe('restored-readonly-page');
 return {observations,disabledLast};
}