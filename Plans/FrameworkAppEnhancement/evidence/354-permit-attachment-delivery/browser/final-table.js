async (page) => {
 const received=page.waitForEvent('download');await page.getByRole('button',{name:'Download verified attachment',exact:true}).click();const download=await received;
 await download.saveAs('C:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/output/playwright/354-document-browser/final-downloaded-report.pdf');
 const table=page.getByRole('table').filter({has:page.getByRole('button',{name:'Download verified attachment',exact:true})});
 const layout=await table.evaluate(table=>{const holder=table.closest('.mud-table-container');return {viewport:innerWidth,pageScroll:document.documentElement.scrollWidth,container:holder.clientWidth,tableScroll:holder.scrollWidth,tableWidth:table.getBoundingClientRect().width,left:table.getBoundingClientRect().left};});
 if(layout.viewport!==390 || layout.pageScroll!==390 || layout.tableScroll>layout.container)throw new Error('Narrow table clipped '+JSON.stringify(layout));
 await table.screenshot({path:'C:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/output/playwright/354-document-browser/narrow-table-final-02.png'});
 await page.screenshot({path:'C:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/output/playwright/354-document-browser/narrow-verified-final-02.png',fullPage:true});
 return {observation:'final-unclipped-390px-attachment-and-download',filename:download.suggestedFilename(),layout,metadata:await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json()};
}
