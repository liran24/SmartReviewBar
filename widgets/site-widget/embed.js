/* SMART STICKY REVIEWER — Site Widget (vanilla JS)
   - Fetches widget data
   - Renders a sticky review bar
   - Fails silently on any error
*/

(function () {
  try {
    const script = document.currentScript;
    const apiBase = (script && script.dataset && script.dataset.apiBase) ? script.dataset.apiBase : "http://localhost:5260";
    const siteId = (script && script.dataset && script.dataset.siteId) ? script.dataset.siteId : null;
    const productId = (script && script.dataset && script.dataset.productId) ? script.dataset.productId : null;

    if (!siteId) return;

    const url = new URL(`${apiBase.replace(/\/+$/, "")}/api/widget/sites/${encodeURIComponent(siteId)}`);
    if (productId) url.searchParams.set("productId", productId);

    fetch(url.toString(), { headers: { "Accept": "application/json" } })
      .then((r) => r.ok ? r.json() : null)
      .then((data) => {
        if (!data || !data.shouldRender) return;

        const root = document.createElement("div");
        root.id = "smart-sticky-reviewer";
        root.style.position = "fixed";
        root.style.left = "16px";
        root.style.right = "16px";
        root.style.bottom = "16px";
        root.style.zIndex = "2147483647";
        root.style.borderRadius = "14px";
        root.style.boxShadow = "0 12px 40px rgba(0,0,0,0.35)";
        root.style.border = "1px solid rgba(255,255,255,0.16)";
        root.style.background = data.backgroundColorHex || "#111827";
        root.style.color = data.textColorHex || "#F9FAFB";
        root.style.padding = "12px 14px";
        root.style.fontFamily = "ui-sans-serif, system-ui, -apple-system, Segoe UI, Roboto, Helvetica, Arial";

        const row = document.createElement("div");
        row.style.display = "flex";
        row.style.alignItems = "center";
        row.style.justifyContent = "space-between";
        row.style.gap = "12px";

        const left = document.createElement("div");
        left.style.display = "flex";
        left.style.flexDirection = "column";
        left.style.gap = "2px";

        const title = document.createElement("div");
        title.textContent = "Trusted reviews";
        title.style.fontSize = "12px";
        title.style.opacity = "0.85";

        const main = document.createElement("div");
        main.style.display = "flex";
        main.style.alignItems = "center";
        main.style.gap = "10px";

        if (typeof data.rating === "number") {
          const stars = document.createElement("div");
          stars.style.display = "inline-flex";
          stars.style.gap = "2px";
          const full = Math.round(Math.max(0, Math.min(5, data.rating)));
          for (let i = 1; i <= 5; i++) {
            const s = document.createElement("span");
            s.textContent = i <= full ? "★" : "☆";
            s.style.color = data.accentColorHex || "#F59E0B";
            s.style.fontSize = "16px";
            stars.appendChild(s);
          }

          const score = document.createElement("span");
          score.textContent = `${data.rating.toFixed(1)}/5`;
          score.style.fontSize = "14px";
          score.style.fontWeight = "600";

          main.appendChild(stars);
          main.appendChild(score);
        }

        const text = document.createElement("span");
        text.textContent = data.text || "";
        text.style.fontSize = "13px";
        text.style.opacity = "0.95";
        if (data.text) main.appendChild(text);

        left.appendChild(title);
        left.appendChild(main);

        const close = document.createElement("button");
        close.type = "button";
        close.textContent = "×";
        close.setAttribute("aria-label", "Close");
        close.style.width = "32px";
        close.style.height = "32px";
        close.style.borderRadius = "10px";
        close.style.border = "1px solid rgba(255,255,255,0.18)";
        close.style.background = "rgba(0,0,0,0.15)";
        close.style.color = data.textColorHex || "#F9FAFB";
        close.style.cursor = "pointer";
        close.style.fontSize = "18px";
        close.onclick = function () {
          try { root.remove(); } catch (_) {}
        };

        row.appendChild(left);
        row.appendChild(close);
        root.appendChild(row);

        // Avoid duplicate injection
        const existing = document.getElementById("smart-sticky-reviewer");
        if (existing) existing.remove();
        document.body.appendChild(root);
      })
      .catch(function () { /* fail silently */ });
  } catch (_) {
    /* fail silently */
  }
})();

