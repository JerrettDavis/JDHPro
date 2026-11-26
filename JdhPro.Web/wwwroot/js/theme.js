/* Synchronous bootstrap before CSS and Blazor: identity and mode are independent. */
(function () {
  'use strict';
  const identities = new Set(['professional', 'vaporwave', 'grayscale', 'monochrome']);
  const modes = new Set(['light', 'dark', 'system']);
  const palettes = {
    professional: { light: 'professional', dark: 'professional-dark' },
    vaporwave: { light: 'vaporwave-light', dark: 'vaporwave' },
    grayscale: { light: 'grayscale', dark: 'grayscale-dark' },
    monochrome: { light: 'monochrome-light', dark: 'monochrome-dark' }
  };
  const legacy = new Map([
    ['professional', ['professional', 'light']],
    ['professional-dark', ['professional', 'dark']],
    ['vaporwave-light', ['vaporwave', 'light']],
    ['vaporwave', ['vaporwave', 'dark']],
    ['grayscale', ['grayscale', 'light']],
    ['grayscale-dark', ['grayscale', 'dark']],
    ['monochrome-light', ['monochrome', 'light']],
    ['monochrome-dark', ['monochrome', 'dark']],
    ['system', ['professional', 'system']]
  ]);
  const storageKey = 'jdhpro.appearance';
  const media = window.matchMedia('(prefers-color-scheme: dark)');
  let identity = 'professional', mode = 'light';

  // Migrate a valid legacy stored choice without allowing malformed storage to block rendering.
  try {
    const old = legacy.get(localStorage.getItem('jdhpro.theme'));
    if (old) [identity, mode] = old;
    try {
      const saved = JSON.parse(localStorage.getItem(storageKey));
      if (identities.has(saved?.identity)) identity = saved.identity;
      if (modes.has(saved?.mode)) mode = saved.mode;
    } catch { /* Ignore invalid saved JSON. */ }
  } catch { /* Storage can be blocked. */ }

  // Explicit query axes win over a legacy theme, then stored/default preferences.
  const params = new URL(location.href).searchParams;
  const requested = legacy.get(params.get('theme'));
  if (requested) [identity, mode] = requested;
  if (identities.has(params.get('identity'))) identity = params.get('identity');
  if (modes.has(params.get('mode'))) mode = params.get('mode');

  const appearance = () => [identity, mode];

  function apply() {
    const resolvedMode = mode === 'system' ? (media.matches ? 'dark' : 'light') : mode;
    const root = document.documentElement;
    root.dataset.identity = identity;
    root.dataset.mode = mode;
    root.dataset.resolvedMode = resolvedMode;
    root.dataset.theme = palettes[identity][resolvedMode];
    root.classList.toggle('dark', resolvedMode === 'dark');
  }
  apply();
  media.addEventListener('change', () => { if (mode === 'system') apply(); });

  function positionPicker(panel) {
    const button = document.getElementById(panel.dataset.appearanceTrigger);
    if (!button) return;
    const margin = 12;
    const viewportWidth = document.documentElement.clientWidth;
    const width = Math.min(242, viewportWidth - margin * 2);
    const rect = button.getBoundingClientRect();
    const left = Math.max(margin, Math.min(rect.right - width, viewportWidth - width - margin));
    const top = rect.bottom + 8;
    panel.style.setProperty('--picker-width', width + 'px');
    panel.style.setProperty('--picker-left', left + 'px');
    panel.style.setProperty('--picker-top', top + 'px');
    panel.style.setProperty('--picker-height', Math.max(0, window.innerHeight - top - margin) + 'px');
  }

  // Both independent menus are native auto popovers. Only one can be open at a time.
  document.addEventListener('beforetoggle', event => {
    if (event.target.matches?.('.brand-appearance-panel') && event.newState === 'open')
      positionPicker(event.target);
  }, true);
  document.addEventListener('toggle', event => {
    const panel = event.target;
    if (!panel.matches?.('.brand-appearance-panel')) return;
    const button = document.getElementById(panel.dataset.appearanceTrigger);
    const open = event.newState === 'open';
    button?.setAttribute('aria-expanded', String(open));
    if (open) panel.querySelector('[aria-pressed="true"]')?.focus({ preventScroll: true });
  }, true);
  window.addEventListener('resize', () => {
    document.querySelectorAll('.brand-appearance-panel:popover-open').forEach(positionPicker);
  });

  window.jdhTheme = {
    getAppearance: appearance,
    setPreference(axis, value) {
      if (axis === 'identity' && identities.has(value)) identity = value;
      else if (axis === 'mode' && modes.has(value)) mode = value;
      else return appearance();

      try { localStorage.setItem(storageKey, JSON.stringify({ identity, mode })); } catch {}
      const url = new URL(location.href);
      if (['theme', 'identity', 'mode'].some(key => url.searchParams.has(key))) {
        url.searchParams.delete('theme');
        url.searchParams.set('identity', identity);
        url.searchParams.set('mode', mode);
        history.replaceState(history.state, '', url);
      }
      apply();
      return appearance();
    },
    closePicker(id) {
      const panel = document.getElementById(id);
      if (!panel?.matches('.brand-appearance-panel')) return;
      if (panel.matches(':popover-open')) panel.hidePopover();
      document.getElementById(panel.dataset.appearanceTrigger)?.focus({ preventScroll: true });
    }
  };
})();
