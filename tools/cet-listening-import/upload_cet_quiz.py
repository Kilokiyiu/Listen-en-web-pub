# -*- coding: utf-8 -*-
"""
Upload parsed CET listening quiz JSON into ListenService via admin API.

Env:
  CET_API_BASE   default https://your-domain.com
  CET_ADMIN_USER admin username
  CET_ADMIN_PASS admin password
  CET_ONLY       optional filter like cet4/2026-06-1 or album name substring
  CET_DRY_RUN    1 = only match albums, no save
"""
from __future__ import annotations

import json
import os
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PARSED = ROOT / "英语资源" / "parsed"

BASE = os.environ.get("CET_API_BASE", "https://your-domain.com").rstrip("/")
USER = os.environ.get("CET_ADMIN_USER", "")
PASS = os.environ.get("CET_ADMIN_PASS", "")
ONLY = os.environ.get("CET_ONLY", "").strip()
DRY = os.environ.get("CET_DRY_RUN", "0") == "1"


def http_json(method: str, url: str, body=None, token: str | None = None):
    data = None
    headers = {
        "Accept": "application/json, text/plain, */*",
        "User-Agent": (
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
            "AppleWebKit/537.36 (KHTML, like Gecko) "
            "Chrome/122.0.0.0 Safari/537.36"
        ),
        "Origin": BASE,
        "Referer": f"{BASE}/",
    }
    if body is not None:
        data = json.dumps(body).encode("utf-8")
        headers["Content-Type"] = "application/json"
    if token:
        headers["Authorization"] = f"Bearer {token}"
    req = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            raw = resp.read().decode("utf-8")
            return json.loads(raw) if raw else None
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"HTTP {e.code} {url}: {err[:800]}") from e


def login() -> str:
    if not USER or not PASS:
        raise SystemExit("Set CET_ADMIN_USER and CET_ADMIN_PASS env vars")
    res = http_json(
        "POST",
        f"{BASE}/api/identity/Login/LoginByUserNameAndPwd",
        {"userName": USER, "password": PASS},
    )
    # token may be string or object
    if isinstance(res, str):
        return res
    if isinstance(res, dict):
        for k in ("token", "Token", "accessToken", "data"):
            if k in res and isinstance(res[k], str):
                return res[k]
            if k == "data" and isinstance(res.get("data"), dict):
                t = res["data"].get("token") or res["data"].get("Token")
                if t:
                    return t
    raise RuntimeError(f"Unexpected login response: {res}")


def get_albums(token: str) -> list[dict]:
    cats = http_json("GET", f"{BASE}/api/listen/Listen/GetCategories", token=token)
    albums = []
    for c in cats or []:
        cid = c.get("id") or c.get("Id")
        code = c.get("code") or c.get("Code")
        rows = http_json(
            "GET",
            f"{BASE}/api/listen/Listen/GetAlbumsByCategoryId?categoryId={cid}",
            token=token,
        )
        for a in rows or []:
            name = a.get("name") or a.get("Name") or {}
            cn = name.get("chinese") or name.get("Chinese") or ""
            albums.append(
                {
                    "id": a.get("id") or a.get("Id"),
                    "nameCn": cn,
                    "code": code,
                }
            )
    return albums


def match_album(albums: list[dict], doc: dict) -> dict | None:
    target = doc["albumNameCn"]
    for a in albums:
        if a["nameCn"] == target:
            return a
    # fuzzy: year month set
    y, m, s = doc["year"], doc["month"], doc["set"]
    level = "四级" if doc["level"] == "cet4" else "六级"
    for a in albums:
        n = a["nameCn"]
        if f"{y}年{m}月" in n and level in n and f"第{s}套" in n:
            return a
    return None


def save_quiz(token: str, album_id: str, sections: list):
    return http_json(
        "POST",
        f"{BASE}/api/listen/Admin/SaveQuizSections",
        {"albumId": album_id, "sections": sections},
        token=token,
    )


def main():
    files = sorted(PARSED.glob("cet*/*.json"))
    files = [f for f in files if f.name != "summary.json"]
    if ONLY:
        files = [f for f in files if ONLY.lower() in str(f).lower() or ONLY in f.read_text(encoding="utf-8")]
    print(f"Parsed files: {len(files)}")
    if not files:
        raise SystemExit("No parsed JSON. Run parse_cet_pdf.py first.")

    token = login()
    albums = get_albums(token)
    print(f"Albums on server: {len(albums)}")

    ok = fail = skip = 0
    for path in files:
        doc = json.loads(path.read_text(encoding="utf-8"))
        album = match_album(albums, doc)
        if not album:
            print(f"NO ALBUM: {doc['albumNameCn']}")
            skip += 1
            continue
        qn = doc.get("questionCount") or sum(len(s.get("questions") or []) for s in doc.get("apiSections") or [])
        if qn < 5:
            print(f"LOW QUALITY skip ({qn} q): {doc['albumNameCn']}")
            skip += 1
            continue
        print(f"{'DRY ' if DRY else ''}UPLOAD {doc['albumNameCn']} -> {album['id']} ({qn} q, {len(doc.get('apiSections') or [])} passages)")
        if DRY:
            ok += 1
            continue
        try:
            save_quiz(token, album["id"], doc["apiSections"])
            ok += 1
            print("  saved")
        except Exception as e:
            fail += 1
            print(f"  FAIL: {e}")

    print(f"Done. ok={ok} fail={fail} skip={skip}")


if __name__ == "__main__":
    main()
