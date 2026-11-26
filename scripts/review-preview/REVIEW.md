# Interactive PR review site

This is a snapshot of the published Blazor WebAssembly application from a specific
review revision. It is not production and not a mockup. The revision.json file
identifies the checked-out commit (for PRs, GitHub's synthetic merge commit),
head/base SHAs and workflow run.

1. Extract the entire ZIP into a folder.
2. From that folder, run `python serve.py` (or `python3 serve.py`; Python 3.10+).
3. Open http://127.0.0.1:4173/ and use the theme selector, navigation and browser
   responsive tools. A different port can be selected with `--port 4174`.
4. Press Ctrl+C in the terminal to stop.

No .NET SDK, Node.js, or npm install is required to run the already-published site.
Do not open site/index.html directly: the WASM runtime requires HTTP. The server
binds to loopback only and handles SPA route reloads, including article slugs.
External web fonts and remote links may still need an internet connection.
Do not submit real contact requests or enter personal data in a preview.
Only execute artifacts from PRs you trust.
