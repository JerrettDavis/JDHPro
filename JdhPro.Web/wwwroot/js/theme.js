/* Synchronous bootstrap: run before CSS and Blazor to avoid a theme flash. */
(function () {
  'use strict';
  const themes = new Set(["professional","professional-dark","vaporwave","vaporwave-light","grayscale","monochrome-light","monochrome-dark"]);
  const dark = new Set(['professional-dark', 'vaporwave', 'monochrome-dark']);
  const storageKey = 'jdhpro.theme';
  const media = window.matchMedia('(prefers-color-scheme: dark)');
  const valid = value => value === 'system' || themes.has(value);
  let choice;
  try {
    const requested = new URL(location.href).searchParams.get('theme');
    if (valid(requested)) choice = requested;
    else {
      const saved = localStorage.getItem(storageKey);
      if (valid(saved)) choice = saved;
    }
  } catch { /* Storage can be blocked; still render a valid theme. */ }
  choice ||= 'professional';

  function apply() {
    const resolved = choice === 'system' ? (media.matches ? 'professional-dark' : 'professional') : choice;
    document.documentElement.dataset.theme = resolved;
    document.documentElement.classList.toggle('dark', dark.has(resolved));
  }
  apply();
  media.addEventListener('change', () => { if (choice === 'system') apply(); });

  // Native auto popover provides light-dismiss and Escape; keep the top-layer
  // panel within the viewport and anchor it to the real trigger.
  const picker = () => document.getElementById('brand-theme-panel');
  const trigger = () => document.getElementById('brand-theme-trigger');

  function positionPicker() {
    const panel = picker(), button = trigger();
    if (!panel || !button) return;
    const margin = 12;
    const width = Math.min(390, window.innerWidth - margin * 2);
    const rect = button.getBoundingClientRect();
    const left = Math.max(margin, Math.min(rect.right - width, window.innerWidth - width - margin));
    const top = rect.bottom + 10;
    panel.style.setProperty('--picker-left', left + 'px');
    panel.style.setProperty('--picker-right', 'auto');
    panel.style.setProperty('--picker-top', top + 'px');
    panel.style.setProperty('--picker-height', Math.max(0, window.innerHeight - top - margin) + 'px');
  }

  document.addEventListener('beforetoggle', event => {
    if (event.target === picker() && event.newState === 'open') positionPicker();
  }, true);
  document.addEventListener('toggle', event => {
    const panel = picker(), button = trigger();
    if (!panel || !button || event.target !== panel) return;
    const open = event.newState === 'open';
    button.setAttribute('aria-expanded', String(open));
    if (open) panel.querySelector('[aria-pressed="true"]')?.focus({ preventScroll: true });
  }, true);
  window.addEventListener('resize', () => {
    if (picker()?.matches(':popover-open')) positionPicker();
  });

  window.jdhTheme = {
    closePicker() {
      const panel = picker();
      if (panel?.matches(':popover-open')) panel.hidePopover();
      trigger()?.focus({ preventScroll: true });
    },
    get: () => choice,
    set(value) {
      if (!valid(value)) return;
      choice = value;
      try { localStorage.setItem(storageKey, value); } catch {}
      const url = new URL(location.href);
      if (url.searchParams.has('theme')) {
        url.searchParams.set('theme', value);
        history.replaceState(history.state, '', url);
      }
      apply();
    }
  };
})();
