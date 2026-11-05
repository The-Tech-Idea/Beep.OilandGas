async (page) => {
 const table=page.getByRole('table').filter({has:page.getByRole('button',{name:'Download verified attachment',exact:true})});
 await table.screenshot({path:'C:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/output/playwright/354-document-browser/narrow-table-final.png'});
 return await table.locator('td').evaluateAll(cells=>cells.map(cell=>({label:cell.getAttribute('data-label'),box:{x:cell.getBoundingClientRect().x,width:cell.getBoundingClientRect().width},before:{width:getComputedStyle(cell,'::before').width,flex:getComputedStyle(cell,'::before').flex,margin:getComputedStyle(cell,'::before').margin,minWidth:getComputedStyle(cell,'::before').minWidth},display:getComputedStyle(cell).display})));
}
