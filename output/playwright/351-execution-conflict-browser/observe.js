async (page) => {
 const metadata=await (await page.request.get(new URL("/fixture/metadata",page.url()).href)).json();
 const storage=await page.evaluate(()=>Object.entries(sessionStorage).filter(([key])=>key.endsWith(".drilling-execution")).map(([key,value])=>({key,length:value.length,plaintext:value.includes("Controlled retained start that was never recorded")})));
 return {text:await page.locator("body").innerText(),metadata,storage};
}