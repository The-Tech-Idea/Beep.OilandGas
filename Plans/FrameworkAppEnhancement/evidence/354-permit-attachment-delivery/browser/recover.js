async (page) => {
 await page.getByRole('button',{name:'Fixture read only',exact:true}).click();
 await page.reload();
 await page.getByRole('button',{name:'Recover retained upload',exact:true}).waitFor();
 await page.getByRole('button',{name:'Recover retained upload',exact:true}).click();
 await page.getByText(/Staged document candidate/).waitFor();
 const first=await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json();
 if(first.documentAttempts!==1 || first.documentEffects!==1 || first.effects!==0)throw new Error('Recovery wrote a new effect');
 const keys=await page.evaluate(()=>Object.keys(sessionStorage));if(keys.some(x=>x.endsWith('.document')) || keys.filter(x=>x.endsWith('.staged')).length!==1)throw new Error('Protected candidate transition not exact');
 if(await page.getByRole('button',{name:'Link reviewed staged document',exact:true}).count())throw new Error('Readonly link exposed');
 await page.reload();await page.getByText(/Staged document candidate/).waitFor();
 return {observation:'reload-readonly-recovery-and-durable-candidate',metadata:first,protectedKeys:keys};
}
