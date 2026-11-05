// Controlled DOM/WebCrypto seams exercise the actual centralized script; no browser claim.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { webcrypto, createHash } = require('node:crypto');

(async () => {
    const bytes = Buffer.from([1, 2, 3]);
    const digest = createHash('sha256').update(bytes).digest('hex').toUpperCase();
    let blobs = 0, clicked = 0, removed = 0, revoked = 0, filename;
    const context = {
        window: {}, crypto: webcrypto, atob, Uint8Array, Blob,
        URL: { createObjectURL(blob) { assert.equal(blob.type, 'application/octet-stream'); blobs++; return 'blob:controlled'; }, revokeObjectURL(url) { assert.equal(url, 'blob:controlled'); revoked++; } },
        setTimeout(callback) { callback(); },
        document: { body: { appendChild() {} }, createElement(tag) { assert.equal(tag, 'a'); return { click() { clicked++; filename = this.download; }, remove() { removed++; } }; } }
    };
    vm.runInNewContext(fs.readFileSync(path.join(__dirname, '../Beep.OilandGas.Web/wwwroot/js/site.js'), 'utf8'), context);
    const deliver = context.window.oilGasDocuments.download;
    await deliver(bytes.toString('base64'), digest, '../../unsafe:<script>.html');
    assert.equal(clicked, 1); assert.equal(removed, 1); assert.equal(revoked, 1);
    assert.match(filename, /^[a-zA-Z0-9._-]+$/);
    await assert.rejects(deliver(Buffer.from([3, 2, 1]).toString('base64'), digest, 'original'), /digest differs/);
    await assert.rejects(deliver('', digest, 'original'), /size is invalid/);
    await assert.rejects(deliver(bytes.toString('base64'), 'forged', 'original'), /proof is required/);
    await assert.rejects(deliver(Buffer.alloc(8388609).toString('base64'), digest, 'original'), /size is invalid/);
    assert.equal(blobs, 1); assert.equal(clicked, 1);
    console.log('Passed: exact attachment bytes/digest, sanitized name, cleanup, altered/empty/malformed/oversized refusal (controlled browser seams).');
})().catch(error => { console.error(error); process.exitCode = 1; });
