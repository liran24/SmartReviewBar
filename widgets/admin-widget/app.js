/* SMART STICKY REVIEWER — Admin Widget (vanilla JS) */

const els = {
  apiBase: document.getElementById("apiBase"),
  siteId: document.getElementById("siteId"),
  loadBtn: document.getElementById("loadBtn"),
  saveBtn: document.getElementById("saveBtn"),
  status: document.getElementById("status"),
  warnings: document.getElementById("warnings"),

  plan: document.getElementById("plan"),
  features: document.getElementById("features"),

  primaryProvider: document.getElementById("primaryProvider"),
  storeOwnerEmail: document.getElementById("storeOwnerEmail"),

  manualRating: document.getElementById("manualRating"),
  manualText: document.getElementById("manualText"),
  fallbackText: document.getElementById("fallbackText"),

  bgColor: document.getElementById("bgColor"),
  textColor: document.getElementById("textColor"),
  accentColor: document.getElementById("accentColor"),

  fallbackTextLock: document.getElementById("fallbackTextLock"),
  styleLock: document.getElementById("styleLock"),
};

function getApiBase() {
  const v = (els.apiBase.value || "").trim();
  return v || "http://localhost:5260";
}

function getSiteId() {
  const v = (els.siteId.value || "").trim();
  if (!v) throw new Error("Site ID is required.");
  return v;
}

function setStatus(text, kind = "info") {
  els.status.textContent = text;
  els.status.style.color =
    kind === "ok" ? "#a7f3d0" :
    kind === "err" ? "#fecaca" :
    "#9ca3af";
}

function renderFeatures(featureAvailability) {
  els.features.innerHTML = "";
  Object.keys(featureAvailability).forEach((k) => {
    const enabled = !!featureAvailability[k];
    const chip = document.createElement("span");
    chip.className = "chip " + (enabled ? "on" : "off");
    chip.textContent = `${k}: ${enabled ? "ON" : "OFF"}`;
    els.features.appendChild(chip);
  });
}

function applyFeatureGating(featureAvailability) {
  const multi = !!featureAvailability["MultipleReviewProviders"];
  const fallbackText = !!featureAvailability["ManualFallbackText"];
  const styling = !!featureAvailability["AdvancedStyling"];

  // Provider selection gating
  if (!multi && els.primaryProvider.value === "JudgeMe") {
    els.primaryProvider.value = "Manual";
  }
  // Keep JudgeMe option visible but disable selection if not available.
  [...els.primaryProvider.options].forEach((opt) => {
    if (opt.value === "JudgeMe") opt.disabled = !multi;
  });

  // Fallback text gating
  els.fallbackText.disabled = !fallbackText;
  els.fallbackTextLock.classList.toggle("visible", !fallbackText);

  // Styling gating
  els.bgColor.disabled = !styling;
  els.textColor.disabled = !styling;
  els.accentColor.disabled = !styling;
  els.styleLock.classList.toggle("visible", !styling);
}

function mapApiConfigToForm(cfg) {
  els.plan.value = cfg.plan;
  els.primaryProvider.value = cfg.primaryProvider;
  els.manualRating.value = cfg.manualRating ?? "";
  els.manualText.value = cfg.manualText ?? "";
  els.fallbackText.value = cfg.fallbackText ?? "";
  els.storeOwnerEmail.value = cfg.storeOwnerEmail ?? "";
  els.bgColor.value = cfg.backgroundColorHex || "#111827";
  els.textColor.value = cfg.textColorHex || "#F9FAFB";
  els.accentColor.value = cfg.accentColorHex || "#F59E0B";
}

function buildSavePayload() {
  const manualRatingRaw = (els.manualRating.value || "").trim();
  const manualRating = manualRatingRaw === "" ? null : Number(manualRatingRaw);

  return {
    plan: els.plan.value,
    primaryProvider: els.primaryProvider.value,
    manualRating: Number.isFinite(manualRating) ? manualRating : null,
    manualText: (els.manualText.value || "").trim() || null,
    fallbackText: (els.fallbackText.value || "").trim() || null,
    storeOwnerEmail: (els.storeOwnerEmail.value || "").trim() || null,
    backgroundColorHex: els.bgColor.value,
    textColorHex: els.textColor.value,
    accentColorHex: els.accentColor.value,
  };
}

async function load() {
  els.warnings.textContent = "";
  setStatus("Loading...");

  const apiBase = getApiBase();
  const siteId = getSiteId();
  localStorage.setItem("ssr_api_base", apiBase);

  const res = await fetch(`${apiBase}/api/admin/sites/${encodeURIComponent(siteId)}/config`, {
    headers: { "Accept": "application/json" },
  });
  if (!res.ok) throw new Error(`Load failed (${res.status})`);

  const data = await res.json();
  mapApiConfigToForm(data.configuration);
  renderFeatures(data.featureAvailability);
  applyFeatureGating(data.featureAvailability);
  setStatus("Loaded.", "ok");
}

async function save() {
  els.warnings.textContent = "";
  setStatus("Saving...");

  const apiBase = getApiBase();
  const siteId = getSiteId();
  localStorage.setItem("ssr_api_base", apiBase);

  const payload = buildSavePayload();

  const res = await fetch(`${apiBase}/api/admin/sites/${encodeURIComponent(siteId)}/config`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", "Accept": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(`Save failed (${res.status}): ${text}`);
  }

  const data = await res.json();
  mapApiConfigToForm(data.configuration);
  if (data.warnings && data.warnings.length) {
    els.warnings.textContent = data.warnings.map((w) => `- ${w}`).join("\n");
  } else {
    els.warnings.textContent = "";
  }

  // Reload computed features after save (plan might have changed)
  await load();

  setStatus("Saved.", "ok");
}

function init() {
  els.apiBase.value = localStorage.getItem("ssr_api_base") || "http://localhost:5260";
  els.siteId.value = "demo-site";

  els.loadBtn.addEventListener("click", () => load().catch((e) => setStatus(e.message, "err")));
  els.saveBtn.addEventListener("click", () => save().catch((e) => setStatus(e.message, "err")));

  els.plan.addEventListener("change", () => {
    // UX: keep form consistent; backend is the authority.
    setStatus("Plan changed. Click Save to apply.");
  });
}

init();

