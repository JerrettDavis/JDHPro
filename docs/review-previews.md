# Review previews

The PR review workflow creates a snapshot of the actual Release/AOT publish
output using the same project settings as GitHub Pages, not a duplicated
prototype. It checks out the PR merge commit so the stacked base changes are
included. The current production deployment is unchanged.

## Reviewing a pull request

Open the **PR review preview** check/run, then the run summary (or its sticky
PR comment for same-repository PRs). Once the run finishes:

- Download **review-gallery**, extract and open index.html. Filter by theme
  and viewport; click an image for the complete screenshot. manifest.json
  records the source revision and per-capture diagnostics. A failed browser
  check is surfaced as a failed workflow, not passed off as a valid preview.
- Download **review-site**, extract and run python serve.py. It serves the
  published site on http://127.0.0.1:4173/ with SPA route support. See the
  artifact README. This is an interactive *local* preview, not a hosted URL.
- Inspect the recorded head/base/merge SHAs before approving. Preview links
  update on new PR events; stale runs are cancelled and stale comments skipped.
  A change to an upstream stack branch can require a fresh PR synchronization
  or rebase. After the workflow exists on the default branch it can also be
  run manually against a selected ref from Actions.

## Screenshot scope

Seven brand themes on desktop (1440 x 900) and mobile (390 x 844), for Home,
Services, Products, Projects, Labs, Insights, About, Contact and a sample
article if the checked-in post catalog is nonempty. All seven homepage themes
also captured at tablet (768 x 1024); one expanded mobile menu capture. The
appearance picker is captured open on desktop and mobile in Professional and
Vaporwave, with keyboard dismissal, focus restoration and selection smoke checks.
Full-page PNGs use real Chromium rendering with reduced motion. The script
waits for WASM startup, actual page headings, content loading and theme
application. It checks service counts and contact form presence, records JS
exceptions and local HTTP failures as failures, and logs console errors and
potential overflow as warnings for visual review. This is a visual smoke test,
not an exhaustive interaction or accessibility audit. It never submits forms.

## Operations and safety

- No Vercel/Cloudflare credentials or Pages deployment; no new paid services.
- All PR base branches are included, so a stacked PR is not filtered out.
  Fork PRs run under GitHub's normal approval policy with read-only permissions;
  the comment job is restricted to same-repo PRs, has no checkout and never
  executes PR application code with a write token.
- Artifacts expire after 14 days; rerun for a fresh snapshot. Downloads
  require GitHub access. No screenshots are committed to the repository.
- GitHub Pages has one production deployment for this repository. Do not use
  it as a preview environment. If hosted, shareable previews become necessary,
  configure a separate preview-only Vercel/Cloudflare project and isolated
  credentials in a follow-up; never use a production target.
- The dedicated workflow is independent of existing Reqnroll/Playwright
  CI. It does not enable or replace the disabled E2E job.
- To run the gallery locally after publishing, install scripts/review-preview/
  requirements.txt and Chromium via python -m playwright install chromium.
  Generate revision.json using metadata.py, then invoke capture.py with
  --site <published wwwroot> --revision <revision.json> --output <folder>.
