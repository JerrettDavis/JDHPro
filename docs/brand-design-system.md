# Brand design system

The default identity is Professional (light). Identity and appearance mode are
independent preferences: Professional, Vaporwave, Grayscale, or Black & White
can each be used in Light, Dark, or System mode. System follows the operating
system for the chosen identity. Eight resolved palettes drive the semantic CSS.
Preferences persist in local storage; existing single-theme preferences migrate
automatically. Shareable URLs use `?identity=vaporwave&mode=system` (or light/dark).
Legacy `?theme=vaporwave` links still resolve to the original dark palette.
Explicit query axes override legacy query values and then stored defaults.
Theme initialization runs before styles and the WASM runtime.

Use the semantic custom properties in wwwroot/css/brand.css for new components.
The legacy utility bridge is transitional; do not introduce new hard-coded
tech-cyan/lime/orange or gray-950 classes in feature components. The grayscale
UI accent is deliberately darker than the logo's original #777777 for text
contrast. Logo geometry comes from the original square icon; the site uses
live, accessible company-name text beside it rather than duplicate initials
in a fixed image. Product brands keep their own identity.

Review checklist:
- Check all eight palettes on every route; test keyboard and touch navigation.
- Review contrast of body text, links, buttons, form validation, code blocks,
  and focus states in light/dark modes.
- Check at 320, 768, 1024, and 1440 CSS pixels and 200% zoom.
- Verify persistence, shareable links, invalid URLs, blocked local storage,
  and System appearance with an OS theme change.
- Check that no loader or motion blocks interaction in reduced-motion mode.
- Run existing build + E2E workflow; new Website themes feature covers basic
  persistence and mobile selector availability. Visual/contrast checks are
  required before merge.

## Appearance controls

The header contains exactly two icon-only appearance controls with accessible
names and 44px hit targets. The first displays the chosen mode's sun, moon or
monitor glyph and opens a three-option menu. The second displays a palette
glyph and opens a four-option identity menu with actual brand swatches.
Both menus use native non-modal auto popovers, checked selection indicators,
keyboard opening, Tab traversal, Escape/outside dismissal, and focus restoration.
Selecting one setting preserves the other. Modern browser popover support is
required; review Safari and Firefox in addition to Chromium.
