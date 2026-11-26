"""Record provenance of the exact tree published by the review workflow."""
import json
import os
from pathlib import Path
import subprocess
import sys
from datetime import datetime, timezone

event_path = os.environ.get("GITHUB_EVENT_PATH")
event = json.loads(Path(event_path).read_text(encoding="utf-8")) if event_path else {}
pr = event.get("pull_request") or {}
data = {
    "revision": subprocess.check_output(["git", "rev-parse", "HEAD"], text=True).strip(),
    "pr": pr.get("number"),
    "head": (pr.get("head") or {}).get("sha"),
    "base": (pr.get("base") or {}).get("sha"),
    "run_id": os.environ.get("GITHUB_RUN_ID"),
    "run_attempt": os.environ.get("GITHUB_RUN_ATTEMPT"),
    "built_utc": datetime.now(timezone.utc).isoformat(),
    "build": "dotnet publish Release (project AOT settings)",
}
Path(sys.argv[1]).write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")
