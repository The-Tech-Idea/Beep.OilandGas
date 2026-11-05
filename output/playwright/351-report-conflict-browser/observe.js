async (page) => {
 const metadata=await(await page.request.get(new URL('/fixture/metadata',page.url()).href)).json();
 const storage=await page.evaluate(()=>Object.entries(sessionStorage).filter(([key])=>key.endsWith('.drilling-report')).map(([key,value])=>({key,length:value.length,plaintext:value.includes('Different unrecorded browser site log 471')})));
 return {text:await page.locator('body').innerText(),metadata,storage};
}