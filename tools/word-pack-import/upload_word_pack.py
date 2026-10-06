#!/usr/bin/env python3
"""
将 parse_vocab_pdf.py 导出的 JSON 上传到 WordService 管理接口。

环境变量:
  WORD_API_BASE   默认 https://your-domain.com/api/word
  ADMIN_TOKEN     管理员 JWT（必填）
  WORD_PACK_CODE  可选；不存在则按 JSON 内 code 创建

用法:
  set ADMIN_TOKEN=...
  python tools/word-pack-import/upload_word_pack.py tools/word-pack-import/out/cet4-core.json
  python tools/word-pack-import/upload_word_pack.py tools/word-pack-import/out --publish
"""

from __future__ import annotations

import argparse
import json
import os
import sys
import urllib.error
import urllib.request
from pathlib import Path

API_BASE = os.environ.get("WORD_API_BASE", "https://your-domain.com/api/word").rstrip("/")
IDENTITY_BASE = os.environ.get(
    "IDENTITY_API_BASE", "https://your-domain.com/api/identity"
).rstrip("/")
TOKEN = os.environ.get("ADMIN_TOKEN", "").strip()


def _headers(token: str | None = None) -> dict[str, str]:
    h = {
        "Content-Type": "application/json",
        "Accept": "application/json, text/plain, */*",
        "User-Agent": (
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
            "AppleWebKit/537.36 (KHTML, like Gecko) "
            "Chrome/122.0.0.0 Safari/537.36"
        ),
        "Origin": "https://your-domain.com",
        "Referer": "https://your-domain.com/",
    }
    if token:
        h["Authorization"] = f"Bearer {token}"
    return h


def login_for_token() -> str:
    global TOKEN
    if TOKEN:
        return TOKEN
    user = os.environ.get("ADMIN_USER", "").strip()
    password = os.environ.get("ADMIN_PASSWORD", "").strip()
    if not user or not password:
        raise SystemExit(
            "请设置 ADMIN_TOKEN，或同时设置 ADMIN_USER + ADMIN_PASSWORD"
        )
    body = json.dumps({"userName": user, "password": password}).encode("utf-8")
    req = urllib.request.Request(
        f"{IDENTITY_BASE}/Login/LoginByUserNameAndPwd",
        data=body,
        method="POST",
        headers=_headers(),
    )
    try:
        with urllib.request.urlopen(req, timeout=30) as resp:
            raw = json.loads(resp.read().decode("utf-8"))
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8", errors="replace")
        raise SystemExit(f"登录失败 HTTP {e.code}: {err}") from e

    # 兼容多种返回形态：{ token, userName }
    token = None
    if isinstance(raw, str):
        token = raw
    elif isinstance(raw, dict):
        token = raw.get("token")
        if not token and isinstance(raw.get("data"), dict):
            token = raw["data"].get("token")
        elif not token and isinstance(raw.get("data"), str):
            token = raw["data"]
    if not token:
        raise SystemExit(f"登录成功但未找到 token，返回: {raw}")
    TOKEN = str(token).strip()
    return TOKEN


def request(method: str, path: str, body: dict | None = None) -> dict:
    token = login_for_token()
    data = None if body is None else json.dumps(body).encode("utf-8")
    req = urllib.request.Request(
        f"{API_BASE}{path}",
        data=data,
        method=method,
        headers=_headers(token),
    )
    try:
        with urllib.request.urlopen(req, timeout=180) as resp:
            raw = resp.read().decode("utf-8")
            return json.loads(raw) if raw else {}
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8", errors="replace")
        raise SystemExit(f"HTTP {e.code} {path}: {err}") from e


def load_payload(path: Path) -> dict:
    data = json.loads(path.read_text(encoding="utf-8"))
    if isinstance(data, list):
        return {
            "code": path.stem,
            "name": path.stem,
            "category": "other",
            "description": "",
            "entries": data,
        }
    return data


def ensure_pack(payload: dict) -> str:
    packs = request("GET", "/Admin/WordPacks").get("data") or []
    code = (payload.get("code") or "").strip().lower()
    for p in packs:
        if (p.get("code") or "").lower() == code:
            # 更新元信息
            request(
                "PUT",
                f"/Admin/WordPacks/{p['id']}",
                {
                    "name": payload.get("name") or p["name"],
                    "category": payload.get("category") or p["category"],
                    "description": payload.get("description") or p.get("description"),
                    "sortOrder": p.get("sortOrder") or 0,
                },
            )
            return p["id"]

    created = request(
        "POST",
        "/Admin/WordPacks",
        {
            "code": code,
            "name": payload.get("name") or code,
            "category": payload.get("category") or "other",
            "description": payload.get("description") or "",
            "sortOrder": payload.get("sortOrder") or 0,
            "isPublished": False,
        },
    )
    return created["data"]["id"]


def import_entries(pack_id: str, entries: list, replace: bool) -> dict:
    # 分批，避免超大 body
    batch = 800
    total = {"added": 0, "updated": 0, "skipped": 0, "total": 0}
    for i in range(0, len(entries), batch):
        chunk = entries[i : i + batch]
        res = request(
            "POST",
            f"/Admin/WordPacks/{pack_id}/entries/import",
            {"entries": chunk, "replaceExisting": replace},
        )
        d = res.get("data") or {}
        total["added"] += d.get("added") or 0
        total["updated"] += d.get("updated") or 0
        total["skipped"] += d.get("skipped") or 0
        total["total"] = d.get("total") or total["total"]
        print(f"  batch {i // batch + 1}: +{d.get('added', 0)} ~{d.get('updated', 0)}")
    return total


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("path", help="JSON 文件或目录")
    parser.add_argument("--replace", action="store_true", help="覆盖已存在释义")
    parser.add_argument("--publish", action="store_true", help="导入后发布")
    args = parser.parse_args()

    target = Path(args.path)
    files = sorted(target.glob("*.json")) if target.is_dir() else [target]
    if not files:
        print("没有 JSON 文件", file=sys.stderr)
        return 1

    for f in files:
        print(f"上传: {f.name}")
        payload = load_payload(f)
        entries = payload.get("entries") or []
        if not entries:
            print("  跳过（无 entries）")
            continue
        pack_id = ensure_pack(payload)
        stats = import_entries(pack_id, entries, args.replace)
        print(f"  done: {stats}")
        if args.publish:
            request("POST", f"/Admin/WordPacks/{pack_id}/publish")
            print("  published")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
