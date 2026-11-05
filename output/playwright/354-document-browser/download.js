async (page) => {
 const received=page.waitForEvent('download');await page.getByRole('button',{name:'Download verified attachment',exact:true}).click();const download=await received;
 await download.saveAs('C:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/output/playwright/354-document-browser/downloaded-report.pdf');
 if(download.suggestedFilename()!=='report.pdf')throw new Error('Wrong filename');
 await page.setViewportSize({width:390,height:844});
 await page.screenshot({path:'C:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/output/playwright/354-document-browser/narrow-verified.png',fullPage:true});
 const dimensions=await page.evaluate(()=>({width:innerWidth,scroll:document.documentElement.scrollWidth}));
 return {observation:'verified-download-and-narrow-layout',filename:download.suggestedFilename(),dimensions};
}
