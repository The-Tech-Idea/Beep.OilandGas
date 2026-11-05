async (page)=>{
 const metadata=await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json();
 const storage=await page.evaluate(()=>Object.entries(sessionStorage).filter(([key])=>key.endsWith('.drilling-execution-assignment')).map(([key,value])=>({key,length:value.length,plaintext:value.includes('Different unrecorded browser reporting original')})));
 return {text:await page.locator('body').innerText(),metadata,storage};
}