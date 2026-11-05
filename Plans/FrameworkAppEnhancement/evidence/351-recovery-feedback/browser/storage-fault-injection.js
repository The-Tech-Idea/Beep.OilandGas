async (page) => {
  await page.addInitScript(() => {
    const get = Storage.prototype.getItem;
    const remove = Storage.prototype.removeItem;
    const owned = key => String(key).endsWith('.drilling-report') || String(key).endsWith('.drilling-execution');
    window.__oilgasBlockReadback = false;
    window.__oilgasBlockRemoval = false;
    Storage.prototype.getItem = function(key) {
      const value = get.call(this, key);
      if (window.__oilgasBlockReadback && owned(key) && value !== null)
        throw new DOMException('Controlled retained-original readback failure', 'SecurityError');
      return value;
    };
    Storage.prototype.removeItem = function(key) {
      if (window.__oilgasBlockRemoval && owned(key))
        throw new DOMException('Controlled retained-original removal failure', 'SecurityError');
      return remove.call(this, key);
    };
  });
  await page.reload();
  return {storageFailureControlsInstalled: true, scope: 'isolated diagnostic browser only'};
}
