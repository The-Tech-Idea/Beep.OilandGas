async (page) => {
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
 await page.getByRole('button',{name:'Fixture lose next acknowledgement',exact:true}).click();
 await page.getByRole('button',{name:'Stage reviewed document',exact:true}).click();
 await page.getByRole('button',{name:'Recover retained upload',exact:true}).waitFor();
 const metadata=await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json();
 if(metadata.documentEffects!==1 || metadata.documentAttempts!==1 || metadata.effects!==0)throw new Error('Unexpected controlled effect population '+JSON.stringify(metadata));
 const keys=await page.evaluate(()=>Object.keys(sessionStorage));if(keys.filter(x=>x.endsWith('.document')).length!==1)throw new Error('Upload original was not retained');
 console.log(JSON.stringify({observation:'file-selected-lost-ack',metadata,protectedKeys:keys}));
}
