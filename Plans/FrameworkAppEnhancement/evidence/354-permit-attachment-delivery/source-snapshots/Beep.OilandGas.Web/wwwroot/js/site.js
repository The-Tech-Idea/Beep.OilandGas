window.oilGasDocuments = {
    async download(base64, expectedDigest, recordedFileId) {
        if (typeof base64 !== "string" || base64.length > 11184812 || !/^[A-F0-9]{64}$/.test(expectedDigest)) throw new Error("Bounded document proof is required.");
        const raw = atob(base64);
        if (!raw.length || raw.length > 8388608) throw new Error("Document content size is invalid.");
        const bytes = Uint8Array.from(raw, value => value.charCodeAt(0));
        let url;
        try {
            const hash = new Uint8Array(await crypto.subtle.digest("SHA-256", bytes));
            const digest = Array.from(hash, value => value.toString(16).padStart(2, "0")).join("").toUpperCase();
            if (digest !== expectedDigest) throw new Error("Original document digest differs.");
            // Attachment only: never execute or embed native HTML/script content in the app.
            url = URL.createObjectURL(new Blob([bytes], { type: "application/octet-stream" }));
            const link = document.createElement("a");
            link.href = url;
            link.download = String(recordedFileId).replace(/[^a-zA-Z0-9._-]/g, "_").slice(0, 128) || "document";
            document.body.appendChild(link);
            try { link.click(); } finally { link.remove(); }
        } finally {
            bytes.fill(0);
            if (url) setTimeout(() => URL.revokeObjectURL(url), 1000);
        }
    }
};