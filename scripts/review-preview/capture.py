"""Capture real published Blazor pages, not a separate mockup."""
import argparse
import html
import json
import os
import re
from pathlib import Path
import threading
from urllib.parse import quote, urlsplit

from playwright.sync_api import sync_playwright, expect
from serve import create_server

PALETTES = {
    "professional": ("professional", "light"),
    "professional-dark": ("professional", "dark"),
    "vaporwave-light": ("vaporwave", "light"),
    "vaporwave": ("vaporwave", "dark"),
    "grayscale": ("grayscale", "light"),
    "grayscale-dark": ("grayscale", "dark"),
    "monochrome-light": ("monochrome", "light"),
    "monochrome-dark": ("monochrome", "dark"),
}
THEMES = tuple(PALETTES)
RESOLVED = {axes: theme for theme, axes in PALETTES.items()}
VIEWPORTS = {"desktop": (1440, 900), "mobile": (390, 844)}
ROUTES = [
    ("home", "/", "Ideas engineered."),
    ("services", "/services", "Our Services"),
    ("products", "/products", "Products & ventures"),
    ("projects", "/projects", "Projects"),
    ("labs", "/labs", "JDH Labs"),
    ("insights", "/blog", "TECHNICAL.INSIGHTS"),
    ("about", "/about", "Building"),
    ("contact", "/contact", "LET'S BUILD"),
]

def routes_for(site):
    routes = list(ROUTES)
    posts_path = site / "data" / "posts.json"
    if posts_path.is_file():
        posts = json.loads(posts_path.read_text(encoding="utf-8-sig"))
        if posts:
            post = posts[0]
            if post.get("id") and post.get("title"):
                routes.append(("article", "/blog/" + quote(post["id"], safe=""), post["title"]))
    return routes

def ready(page, heading):
    expect(page.locator("body")).to_have_class(
        re.compile(r"\bblazor-loaded\b"), timeout=60000)
    expect(page.locator("#app h1").first).to_be_visible(timeout=30000)
    expect(page.locator("#blazor-error-ui")).not_to_be_visible()
    expect(page.locator("#app h1").first).to_contain_text(heading, timeout=15000)
    expect(page.locator("#app .animate-spin").first).not_to_be_visible(timeout=30000)
    # A slow/offline external font must not hang the review.
    page.evaluate("Promise.race([document.fonts.ready, new Promise(r => setTimeout(r, 5000))])")

def write_gallery(output, revision, results):
    escape = lambda value: html.escape(str(value), quote=True)
    failed = sum(row["status"] == "failed" for row in results)
    warning = sum(bool(row.get("warnings")) for row in results)
    cards = []
    for row in results:
        title = escape(f'{row["theme"]} / {row["viewport"]} / {row["route"]}')
        details = row.get("errors", []) + row.get("warnings", [])
        image = row.get("image")
        preview = (f'<a href="{escape(image)}"><img src="{escape(image)}" loading="lazy" alt="{title}"></a>'
                   if image else '<p class="missing">No screenshot available.</p>')
        notes = "".join(f"<li>{escape(note)}</li>" for note in details)
        cards.append(f'<article data-theme="{escape(row["theme"])}" data-viewport="{escape(row["viewport"])}">'
                     f'<h2>{title}</h2><p class="{row["status"]}">{escape(row["status"])}</p>'
                     f'{preview}<p><code>{escape(row["path"])}</code></p>'
                     f'<ul>{notes}</ul></article>')
    theme_options = "".join(f'<option>{escape(t)}</option>' for t in THEMES)
    document = """<!doctype html><html lang="en"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>JDHPro review gallery</title><style>
:root{font:16px/1.5 system-ui,sans-serif;color:#17232d;background:#eef2f4}
body{margin:0 auto;padding:24px;max-width:1600px}h1{margin:0}
header{margin-bottom:24px}label{display:inline-block;margin-right:16px}
select{font:inherit;padding:6px;margin-left:6px}code{overflow-wrap:anywhere}
main{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,320px),1fr));gap:18px}
article{background:white;border:1px solid #c9d5da;border-radius:10px;padding:12px;min-width:0}
article[hidden]{display:none}h2{font-size:1rem;margin:0}
img{display:block;width:100%;height:320px;object-fit:contain;object-position:top;background:#dce4e8}
.passed{color:#136a37}.failed,.missing{color:#a32c24}ul{font-size:.85rem;overflow-wrap:anywhere}
a{color:#005e68}pre{white-space:pre-wrap;overflow-wrap:anywhere}
</style></head><body><header><h1>JDH Productions — review gallery</h1>
"""
    document += (f"<p>{len(results)} captures/checks; {failed} failed; {warning} with warnings. "
                 "Click any image to view the full-size PNG.</p>"
                 f"<details><summary>Build provenance</summary><pre>{escape(json.dumps(revision, indent=2))}</pre></details>"
                 '<p><a href="manifest.json">Raw diagnostics (JSON)</a></p>'
                 '<label>Theme <select id="theme"><option value="">All</option>' + theme_options + '</select></label>'
                 '<label>Viewport <select id="viewport"><option value="">All</option>'
                 '<option>desktop</option><option>mobile</option><option>tablet</option></select></label>'
                 "</header><main>" + "".join(cards) + "</main>")
    document += """<script>
const theme = document.querySelector('#theme'), viewport = document.querySelector('#viewport');
function filter() {
  document.querySelectorAll('article').forEach(card => {
    card.hidden = Boolean((theme.value && card.dataset.theme !== theme.value)
      || (viewport.value && card.dataset.viewport !== viewport.value));
  });
}
theme.addEventListener('change', filter); viewport.addEventListener('change', filter);
</script></body></html>"""
    (output / "index.html").write_text(document, encoding="utf-8")
    (output / "manifest.json").write_text(json.dumps({"revision": revision, "results": results}, indent=2), encoding="utf-8")

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--site", type=Path, required=True)
    parser.add_argument("--revision", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    (args.output / "screenshots").mkdir(exist_ok=True)
    revision = json.loads(args.revision.read_text(encoding="utf-8"))
    results = []
    routes = routes_for(args.site)
    # Ensure a manifest exists even if browser initialization fails.
    write_gallery(args.output, revision, results)
    with create_server(args.site, 0) as server:
        base = f"http://127.0.0.1:{server.server_port}"
        threading.Thread(target=server.serve_forever, daemon=True).start()
        try:
            with sync_playwright() as p:
                browser = p.chromium.launch()
                try:
                    sizes = {**VIEWPORTS, "tablet": (768, 1024)}
                    for viewport, (width, height) in sizes.items():
                        context = browser.new_context(viewport={"width": width, "height": height},
                                                      device_scale_factor=1, reduced_motion="reduce",
                                                      is_mobile=viewport == "mobile", has_touch=viewport != "desktop")
                        page = context.new_page()
                        page.set_default_timeout(15000)
                        errors, warnings = [], []
                        page.on("pageerror", lambda error: errors.append(f"JavaScript: {error}"))
                        page.on("console", lambda message: warnings.append(f"Console: {message.text}") if message.type == "error" else None)
                        page.on("response", lambda response: errors.append(f"HTTP {response.status}: {urlsplit(response.url).path}")
                                if response.url.startswith(base + "/") and response.status >= 400 and not response.url.endswith("/favicon.ico") else None)
                        for theme in THEMES:
                            for name, route, heading in (routes[:1] if viewport == "tablet" else routes):
                                errors.clear()
                                warnings.clear()
                                path = f"{route}?theme={theme}"
                                image_path = f"screenshots/{theme}--{viewport}--{name}.png"
                                result = {"theme": theme, "viewport": viewport, "route": name,
                                          "path": path, "size": [width, height]}
                                try:
                                    response = page.goto(base + path, wait_until="domcontentloaded", timeout=30000)
                                    if not response or not response.ok:
                                        raise RuntimeError("Page navigation did not return HTTP 2xx")
                                    ready(page, heading)
                                    expect(page.locator("html")).to_have_attribute("data-theme", theme)
                                    identity, mode = PALETTES[theme]
                                    expect(page.get_by_test_id("identity-trigger")).to_have_attribute("data-identity", identity)
                                    expect(page.get_by_test_id("mode-trigger")).to_have_attribute("data-mode", mode)
                                    if name == "home":
                                        expect(page.get_by_test_id("service-card")).to_have_count(4)
                                    elif name == "services":
                                        expect(page.get_by_test_id("service-section")).to_have_count(4)
                                    elif name == "contact":
                                        expect(page.locator("#app form")).to_be_visible()
                                    overflow = page.evaluate("""() => Math.max(document.documentElement.scrollWidth,
                                      document.body.scrollWidth) - document.documentElement.clientWidth""")
                                    if overflow > 2:
                                        warnings.append(f"Potential horizontal overflow: {overflow}px")
                                    page.screenshot(path=str(args.output / image_path), full_page=True, animations="disabled",
                                                    timeout=30000)
                                    result["image"] = image_path
                                except Exception as exc:
                                    errors.append(str(exc)[:1500])
                                    # Capture failures too, but distinguish them from valid screenshots.
                                    try:
                                        page.screenshot(path=str(args.output / image_path), full_page=False, timeout=5000)
                                        result["image"] = image_path
                                    except Exception:
                                        pass
                                result.update(status="failed" if errors else "passed",
                                              errors=list(dict.fromkeys(errors)), warnings=list(dict.fromkeys(warnings)))
                                results.append(result)
                                write_gallery(args.output, revision, results)
                                print(f'{result["status"]}: {theme} {viewport} {name}', flush=True)
                        # Review both independent icon menus on light and dark sites.
                        if viewport in ("desktop", "mobile"):
                            for theme in ("professional", "vaporwave"):
                                identity, mode = PALETTES[theme]
                                for axis, selected in (("mode", mode), ("identity", identity)):
                                    errors.clear()
                                    warnings.clear()
                                    result = {"theme": theme, "viewport": viewport, "route": f"{axis}-picker",
                                              "path": f"/?theme={theme}", "size": [width, height]}
                                    image_path = f"screenshots/{theme}--{viewport}--{axis}-picker.png"
                                    try:
                                        page.goto(base + result["path"], wait_until="domcontentloaded")
                                        ready(page, ROUTES[0][2])
                                        trigger = page.get_by_test_id(f"{axis}-trigger")
                                        panel = page.get_by_test_id(f"{axis}-panel")
                                        expect(trigger).to_have_attribute(f"data-{axis}", selected)
                                        expect(trigger).to_be_enabled()
                                        if viewport == "desktop":
                                            trigger.focus()
                                            trigger.press("Enter")
                                        else:
                                            trigger.click()
                                        expect(panel).to_be_visible()
                                        expect(trigger).to_have_attribute("aria-expanded", "true")
                                        expect(page.get_by_test_id(f"{axis}-option-{selected}")).to_be_focused()
                                        bounds = panel.bounding_box()
                                        if not bounds or bounds["x"] < 0 or bounds["y"] < 0 or bounds["x"] + bounds["width"] > width + 1 or bounds["y"] + bounds["height"] > height + 1:
                                            raise RuntimeError("Appearance menu extends outside the viewport")
                                        page.screenshot(path=str(args.output / image_path), animations="disabled")
                                        result["image"] = image_path
                                        page.keyboard.press("Escape")
                                        expect(panel).not_to_be_visible()
                                        expect(trigger).to_be_focused()
                                        trigger.click()
                                        next_value = ("dark" if mode == "light" else "light") if axis == "mode" else "grayscale"
                                        page.get_by_test_id(f"{axis}-option-{next_value}").click()
                                        expect(panel).not_to_be_visible()
                                        expected_identity = next_value if axis == "identity" else identity
                                        expected_mode = next_value if axis == "mode" else mode
                                        expect(page.locator("html")).to_have_attribute("data-theme", RESOLVED[(expected_identity, expected_mode)])
                                        expect(page.get_by_test_id("identity-trigger")).to_have_attribute("data-identity", expected_identity)
                                        expect(page.get_by_test_id("mode-trigger")).to_have_attribute("data-mode", expected_mode)
                                        expect(trigger).to_be_focused()
                                        # Canonical identity/mode URLs and persisted selections survive reload.
                                        page.reload(wait_until="domcontentloaded")
                                        ready(page, ROUTES[0][2])
                                        expect(page.locator("html")).to_have_attribute("data-theme", RESOLVED[(expected_identity, expected_mode)])
                                    except Exception as exc:
                                        errors.append(str(exc)[:1500])
                                    result.update(status="failed" if errors else "passed", errors=list(errors), warnings=list(warnings))
                                    results.append(result)
                                    write_gallery(args.output, revision, results)

                        # System mode must resolve independently of the selected identity.
                        if viewport == "desktop":
                            errors.clear()
                            warnings.clear()
                            result = {"theme": "vaporwave", "viewport": viewport, "route": "system-mode",
                                      "path": "/?identity=vaporwave&mode=system", "size": [width, height]}
                            try:
                                page.emulate_media(color_scheme="dark")
                                page.goto(base + result["path"], wait_until="domcontentloaded")
                                ready(page, ROUTES[0][2])
                                expect(page.locator("html")).to_have_attribute("data-theme", "vaporwave")
                                expect(page.get_by_test_id("mode-trigger")).to_have_attribute("data-mode", "system")
                                page.emulate_media(color_scheme="light")
                                expect(page.locator("html")).to_have_attribute("data-theme", "vaporwave-light")
                                expect(page.get_by_test_id("identity-trigger")).to_have_attribute("data-identity", "vaporwave")
                                image_path = "screenshots/vaporwave--desktop--system-mode.png"
                                page.screenshot(path=str(args.output / image_path), animations="disabled")
                                result["image"] = image_path
                            except Exception as exc:
                                errors.append(str(exc)[:1500])
                            result.update(status="failed" if errors else "passed", errors=list(errors), warnings=list(warnings))
                            results.append(result)
                            write_gallery(args.output, revision, results)
                            page.emulate_media(color_scheme="light")
                        # Show actual expanded navigation (all eight palettes on mobile are captured above).
                        if viewport == "mobile":
                            errors.clear()
                            warnings.clear()
                            result = {"theme": "professional", "viewport": viewport, "route": "mobile-menu",
                                      "path": "/?theme=professional", "size": [width, height]}
                            image_path = "screenshots/professional--mobile--menu.png"
                            try:
                                page.goto(base + result["path"], wait_until="domcontentloaded")
                                ready(page, ROUTES[0][2])
                                page.get_by_test_id("mobile-menu-button").click()
                                expect(page.get_by_test_id("mobile-menu")).to_be_visible()
                                expect(page.get_by_test_id("mobile-menu-button")).to_have_attribute("aria-expanded", "true")
                                page.screenshot(path=str(args.output / image_path), animations="disabled")
                                result["image"] = image_path
                            except Exception as exc:
                                errors.append(str(exc)[:1500])
                            result.update(status="failed" if errors else "passed", errors=list(errors), warnings=list(warnings))
                            results.append(result)
                            write_gallery(args.output, revision, results)
                        context.close()
                finally:
                    browser.close()
        finally:
            server.shutdown()
    failed = sum(row["status"] == "failed" for row in results)
    warning = sum(bool(row.get("warnings")) for row in results)
    if summary := os.environ.get("GITHUB_STEP_SUMMARY"):
        with open(summary, "a", encoding="utf-8") as handle:
            handle.write(f"## Browser review\n\n{len(results)} captures/checks; {failed} failed; {warning} with warnings.\n")
    if not results or failed:
        raise SystemExit(1)

if __name__ == "__main__":
    main()
