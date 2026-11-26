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

  window.jdhTheme = {
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
