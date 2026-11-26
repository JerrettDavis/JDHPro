"""Serve the published Blazor SPA over loopback, including direct route reloads."""
import argparse
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import urlsplit

class PreviewHandler(SimpleHTTPRequestHandler):
    extensions_map = {**SimpleHTTPRequestHandler.extensions_map,
                      ".wasm": "application/wasm", ".mjs": "text/javascript",
                      ".json": "application/json", ".dll": "application/octet-stream"}

    def send_head(self):
        route = urlsplit(self.path).path
        # Route only HTML navigation requests to the SPA; missing assets stay 404.
        if (not Path(self.translate_path(self.path)).exists()
                and "text/html" in self.headers.get("Accept", "")
                and not route.startswith(("/_framework/", "/css/", "/js/", "/content/", "/data/"))
                and ("." not in route.rsplit("/", 1)[-1] or route.startswith("/blog/"))):
            self.path = "/index.html"
        return super().send_head()

    def end_headers(self):
        self.send_header("X-Robots-Tag", "noindex, nofollow")
        super().end_headers()

def create_server(site, port=4173):
    site = Path(site).resolve()
    if not (site / "index.html").is_file():
        raise FileNotFoundError(f"No published index.html in {site}")
    return ThreadingHTTPServer(("127.0.0.1", port),
                               partial(PreviewHandler, directory=str(site)))

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--site", type=Path, default=Path(__file__).resolve().parent / "site")
    parser.add_argument("--port", type=int, default=4173)
    args = parser.parse_args()
    with create_server(args.site, args.port) as server:
        print(f"Review site: http://127.0.0.1:{server.server_port}/", flush=True)
        print("Local preview only. Do not submit real contact requests. Ctrl+C to stop.", flush=True)
        try:
            server.serve_forever()
        except KeyboardInterrupt:
            pass

if __name__ == "__main__":
    main()
