async (page) => {
 await page.getByRole('button',{name:'Review application for workflow',exact:true}).click();
 await page.getByLabel('Choose document file',{exact:true}).setInputFiles('C:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/output/playwright/354-document-browser/report.pdf');
 await page.getByRole('textbox',{name:'Document title*',exact:true}).fill('Reviewed browser document');
 await page.getByRole('textbox',{name:'Native document type code*',exact:true}).fill('PLAN');
 await page.getByRole('textbox',{name:'Recorded language code*',exact:true}).fill('en');
 await page.getByRole('textbox',{name:'Explicit document version*',exact:true}).fill('1');
 await page.getByRole('textbox',{name:'Native digital format code*',exact:true}).fill('PDF');
 await page.getByRole('textbox',{name:'Native information item subtype*',exact:true}).fill('DOC');
 await page.getByRole('textbox',{name:'Effective from UTC (ISO8601 with Z)*',exact:true}).fill('2026-10-28T00:00:00Z');
 await page.getByRole('textbox',{name:'Effective to UTC, exclusive (ISO8601 with Z)*',exact:true}).fill('2027-10-28T00:00:00Z');
 await page.getByRole('textbox',{name:'Reason for this permit action*',exact:true}).fill('Reviewed the selected document against the exact application source.');
 await page.getByRole('checkbox',{name:'I checked this application, country, authority and recorded history for this action.',exact:true}).check();

 await page.getByRole('button',{name:'Stage reviewed document',exact:true}).click();

 await page.getByText(/Staged document candidate/).waitFor();
 await page.getByRole('button',{name:'Review application for workflow',exact:true}).click();
 await page.getByRole('textbox',{name:'Reason for this permit action*',exact:true}).fill('Reviewed the exact staged candidate and application for the narrow display gate.');
 await page.getByRole('checkbox',{name:'I checked this application, country, authority and recorded history for this action.',exact:true}).check();
 await page.getByRole('button',{name:'Link reviewed staged document',exact:true}).click();
 await page.waitForFunction(()=>!Object.keys(sessionStorage).some(x=>x.startsWith('oilgas.permits.original.')));
 await page.getByRole('button',{name:'Review application for workflow',exact:true}).click();
 await page.getByRole('button',{name:'Download verified attachment',exact:true}).waitFor();
 const received=page.waitForEvent('download');await page.getByRole('button',{name:'Download verified attachment',exact:true}).click();const download=await received;
 await download.saveAs('C:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/output/playwright/354-document-browser/final-downloaded-report.pdf');
 await page.screenshot({path:'C:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/output/playwright/354-document-browser/narrow-verified-final.png',fullPage:true});
 const dimensions=await page.evaluate(()=>({width:innerWidth,scroll:document.documentElement.scrollWidth}));
 if(dimensions.width!==390 || dimensions.scroll!==390)throw new Error('Narrow layout overflow '+JSON.stringify(dimensions));
 const cell=page.getByRole('cell').filter({has:page.getByRole('button',{name:'Download verified attachment',exact:true})});
 const content=cell.locator('.permit-attachment-value');const box=await content.boundingBox();if(!box || box.width<130)throw new Error('Grouped content squeezed '+JSON.stringify(box));
 const metadata=await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json();if(metadata.effects!==1 || metadata.documentEffects!==1)throw new Error('Wrong fresh-host effect count');
 return {observation:'rebuilt-grouped-narrow-attachment-and-exact-download',dimensions,contentBox:box,filename:download.suggestedFilename(),metadata};
}
