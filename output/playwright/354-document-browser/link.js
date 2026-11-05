async (page) => {
 await page.getByRole('button',{name:'Reload recorded application',exact:true}).click();
 await page.getByRole('button',{name:'Review application for workflow',exact:true}).click();
 await page.getByRole('button',{name:'Link reviewed staged document',exact:true}).waitFor();
 if(await page.getByLabel('Choose document file',{exact:true}).count())throw new Error('New upload exposed while candidate is unlinked');
 await page.getByRole('textbox',{name:'Reason for this permit action*',exact:true}).fill('Reviewed this exact staged version and current application before linking.');
 await page.getByRole('checkbox',{name:'I checked this application, country, authority and recorded history for this action.',exact:true}).check();
 await page.getByRole('button',{name:'Link reviewed staged document',exact:true}).click();
 await page.getByRole('button',{name:'Review application for workflow',exact:true}).waitFor({state:'visible'});
 // Wait for exact protected cleanup, not merely receipt arrival.
 await page.waitForFunction(()=>!Object.keys(sessionStorage).some(x=>x.startsWith('oilgas.permits.original.')));
 const metadata=await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json();
 if(metadata.effects!==1 || metadata.attempts!==1 || metadata.documentEffects!==1 || metadata.documentAttempts!==1)throw new Error('Unexpected link effects '+JSON.stringify(metadata));
 await page.getByRole('button',{name:'Review application for workflow',exact:true}).click();
 await page.getByRole('button',{name:'Download verified attachment',exact:true}).waitFor();
 return {observation:'author-exact-candidate-link-and-current-verified-read',metadata};
}
