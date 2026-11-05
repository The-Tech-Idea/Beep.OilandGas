async(page)=>{
 await page.setViewportSize({width:1280,height:900});
 const table=page.getByRole('table').filter({has:page.getByRole('button',{name:'Download verified attachment',exact:true})});
 const layout=await table.evaluate(t=>({width:innerWidth,pageScroll:document.documentElement.scrollWidth,tableWidth:t.getBoundingClientRect().width,holderWidth:t.closest('.mud-table-container').clientWidth,holderScroll:t.closest('.mud-table-container').scrollWidth}));
 if(layout.width!==layout.pageScroll || layout.holderScroll>layout.holderWidth)throw new Error('Desktop table overflow '+JSON.stringify(layout));
 await table.screenshot({path:'C:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/output/playwright/354-document-browser/desktop-table-final.png'});
 return {observation:'final-1280px-table',layout};
}
