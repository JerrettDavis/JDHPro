# Brand design system

The default identity is Professional (light). The site offers seven fixed palettes
and a System appearance option that chooses Professional light/dark using the OS.
An allowlisted `?theme=` query parameter overrides saved preference for shareable
previews; manual selection persists in local storage and updates an existing
theme query parameter. Storage failure falls back safely. Theme initialization
runs before styles and the WASM runtime.

Use the semantic custom properties in wwwroot/css/brand.css for new components.
The legacy utility bridge is transitional; do not introduce new hard-coded
tech-cyan/lime/orange or gray-950 classes in feature components. The grayscale
UI accent is deliberately darker than the logo's original #777777 for text
contrast. Logo geometry comes from the original square icon; the site uses
live, accessible company-name text beside it rather than duplicate initials
in a fixed image. Product brands keep their own identity.

Review checklist:
- Check all seven palettes on every route; test keyboard and touch navigation.
- Review contrast of body text, links, buttons, form validation, code blocks,
  and focus states in light/dark modes.
- Check at 320, 768, 1024, and 1440 CSS pixels and 200% zoom.
- Verify persistence, shareable links, invalid URLs, blocked local storage,
  and System appearance with an OS theme change.
- Check that no loader or motion blocks interaction in reduced-motion mode.
- Run existing build + E2E workflow; new Website themes feature covers basic
  persistence and mobile selector availability. Visual/contrast checks are
  required before merge.
