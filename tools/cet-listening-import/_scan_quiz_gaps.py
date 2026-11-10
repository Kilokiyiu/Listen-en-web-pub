# -*- coding: utf-8 -*-
"""Scan online CET albums for missing/low quiz; compare with parsed JSON."""
from __future__ import annotations

import json
import os
import re
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PARSED = ROOT / "英语资源" / "parsed"
BASE = os.environ.get("CET_API_BASE", "https://your-domain.com").rstrip("/")
OUT = ROOT / "英语资源" / "整卷资源" / "_quiz_gaps.txt"

UA = {
    "Accept": "application/json",
    "User-Agent": "Mozilla/5.0",
    "Cache-Control": "no-cache",
}


def http_json(url: str):
    req = urllib.request.Request(url, headers=UA)
    with urllib.request.urlopen(req, timeout=60) as resp:
        return json.loads(resp.read().decode("utf-8"))


def parse_key(name: str, code: str) -> str | None:
    m = re.search(r"(\d{4})\s*年\s*(\d{1,2})\s*月.*?第\s*(\d)\s*套", name)
    if not m:
        return None
    y, mo, s = int(m.group(1)), int(m.group(2)), int(m.group(3))
    return f"{code}/{y}-{mo:02d}-{s}"


def main():
    cats = http_json(f"{BASE}/api/listen/Listen/GetCategories")
    lines = []
    missing = []
    low = []
    ok = []
    parsed_keys = set()
    for p in PARSED.glob("cet*/*.json"):
        if p.name == "summary.json":
            continue
        # folder is cet4/cet6, name is 2024-06-1.json
        level = p.parent.name
        parsed_keys.add(f"{level}/{p.stem}")

    for cat in cats:
        code = (cat.get("code") or "").lower()
        if code not in ("cet4", "cet6"):
            continue
        albums = http_json(
            f"{BASE}/api/listen/Listen/GetAlbumsByCategoryId?categoryId={cat['id']}&_={code}"
        )
        for a in albums:
            name = (a.get("name") or {}).get("chinese") or ""
            aid = a.get("id")
            key = parse_key(name, code) or "?"
            quiz = http_json(f"{BASE}/api/listen/Listen/GetQuizByAlbumId?albumId={aid}&_={aid}")
            sections = quiz.get("sections") or quiz.get("Sections") or []
            qn = 0
            for s in sections:
                qs = s.get("questions") or s.get("Questions") or []
                qn += len(qs)
            has_parsed = key in parsed_keys
            status = "OK" if qn >= 25 else ("LOW" if qn > 0 else "NONE")
            row = f"{status}\tq={qn:2d}\tparsed={'Y' if has_parsed else 'N'}\t{key}\t{name}"
            lines.append(row)
            if status == "NONE":
                missing.append((key, name, has_parsed, qn))
            elif status == "LOW":
                low.append((key, name, has_parsed, qn))
            else:
                ok.append(key)

    summary = [
        f"ok(>=25)={len(ok)} low={len(low)} none={len(missing)} parsed_json={len(parsed_keys)}",
        "",
        "=== NONE (no quiz) ===",
    ]
    for key, name, has_parsed, qn in missing:
        need = "CAN_UPLOAD" if has_parsed else "NEED_听力真题及答案解析PDF"
        summary.append(f"{need}\t{key}\t{name}")
    summary.append("")
    summary.append("=== LOW (<25 q) ===")
    for key, name, has_parsed, qn in low:
        need = "CAN_UPLOAD" if has_parsed else "NEED_听力真题及答案解析PDF"
        summary.append(f"{need}\tq={qn}\t{key}\t{name}")
    summary.append("")
    summary.append("=== detail ===")
    summary.extend(lines)

    text = "\n".join(summary)
    OUT.write_text(text, encoding="utf-8")
    print(text.split("=== detail ===")[0])
    print(f"wrote {OUT}")


if __name__ == "__main__":
    main()
