async (page) => {
 const observations=[];
 const button=()=>page.getByRole('button',{name:'Inspect independent completion routing',exact:true});
 const metadata=async()=>await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json();
 const observe=async label=>observations.push({label,text:await page.locator('body').innerText(),metadata:await metadata(),disabled:await button().count()?await button().isDisabled():null});
 const mode=async value=>{const response=await page.request.post(new URL('/fixture/completion-routing/'+value,page.url()).href);if(!response.ok())throw new Error('Diagnostic routing state unavailable');};
 const ready=async()=>page.getByText('Every configured stage has an eligible independent recipient.',{exact:true}).waitFor({state:'visible'});
 const reload=async()=>{await page.getByRole('button',{name:'Reload original report history',exact:true}).click();await page.getByRole('heading',{name:'Current completion report, pending independent acceptance',exact:true}).waitFor({state:'visible'});};
 const start=await metadata();if(start.recordedReports!==4 || start.completionRoutingReads!==1)throw new Error('Inspect the four-entry controlled claim and initial explicit routing read first');
 await observe('initial-explicit-ready-routing');
 await button().focus();await page.keyboard.press('Enter');await ready();await observe('keyboard-ready-routing');
 await page.screenshot({path:'output/playwright/351-completion-routing-browser/routing-ready.png',fullPage:true});
 await mode('blocked');await button().click();await page.getByText('No eligible independent recipient',{exact:true}).waitFor({state:'visible'});await observe('configured-stage-without-independent-recipient');
 await page.screenshot({path:'output/playwright/351-completion-routing-browser/routing-blocked.png',fullPage:true});
 await mode('empty');await button().click();await page.getByText('No completion approval workflow is configured for this source.',{exact:false}).waitFor({state:'visible'});await observe('no-configured-completion-workflow');
 await mode('outage');await button().click();await page.getByRole('alert').filter({hasText:'The current completion source, configured approval stages and independent recipients could not be confirmed'}).waitFor({state:'visible'});await observe('routing-unavailable-is-error');
 await mode('ready');await button().click();await ready();await observe('restored-ready-routing');
 await page.getByRole('button',{name:'Switch other company',exact:true}).click();await page.getByText('INDEPENDENT_COMPLETION / completion-inspector',{exact:true}).waitFor({state:'hidden'});await observe('other-company-hides-private-routing');
 await page.getByRole('button',{name:'Restore original company/account',exact:true}).click();await reload();await observe('restored-company-requires-explicit-routing-read');
 await page.getByRole('button',{name:'Use read-only unmapped profile',exact:true}).click();await reload();await observe('readonly-routing-disabled-no-private-recipients');
 await page.getByRole('button',{name:'Use editable mapped profile',exact:true}).click();await reload();await button().focus();await page.keyboard.press('Enter');await ready();await observe('editable-keyboard-routing-restored');
 return {scope:'Controlled canonical claim/routing responses; actual report parent/state/component connection. No native recording, submitted review or acceptance is claimed.',observations};
}
