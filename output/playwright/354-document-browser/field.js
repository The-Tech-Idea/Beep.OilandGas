async (page) => {
 await page.getByRole('button',{name:'Fixture other FIELD',exact:true}).click();
 await page.getByText('Company, account or FIELD changed. Reload the recorded application.',{exact:true}).waitFor();
 if(await page.getByRole('button',{name:'Download verified attachment',exact:true}).count())throw new Error('Old download survived FIELD change');
 if((await page.locator('body').innerText()).includes('5A44F143BD89E639AB6BD15D97C5989B3D5F58F4D7C0055F884BD781E0032468'))throw new Error('Old private document evidence survived FIELD change');
 return {observation:'field-change-withholds-private-document-and-download',metadata:await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json()};
}
