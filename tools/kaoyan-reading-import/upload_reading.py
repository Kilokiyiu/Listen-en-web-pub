# -*- coding: utf-8 -*-
"""
上传 parsed/kaoyan/*-reading.json：保留已有完形，替换/写入阅读分区。

Env:
  KAOYAN_API_BASE   default https://your-domain.com
  KAOYAN_ADMIN_USER
  KAOYAN_ADMIN_PASS
  KAOYAN_ONLY       e.g. 2024-eng1
  KAOYAN_DRY_RUN    1 = 只校验
"""
from __future__ import annotations

import json
import os
import time
import urllib.error
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PARSED = ROOT / "英语资源" / "parsed" / "kaoyan"

BASE = os.environ.get("KAOYAN_API_BASE", "https://your-domain.com").rstrip("/")
USER = os.environ.get("KAOYAN_ADMIN_USER", "")
PASS = os.environ.get("KAOYAN_ADMIN_PASS", "")
ONLY = os.environ.get("KAOYAN_ONLY", "").strip()
DRY = os.environ.get("KAOYAN_DRY_RUN", "0") == "1"


def http_json(method: str, url: str, body=None, token: str | None = None, retries: int = 3):
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
    last_err: Exception | None = None
    for attempt in range(retries):
        req = urllib.request.Request(url, data=data, headers=headers, method=method)
        try:
            with urllib.request.urlopen(req, timeout=90) as resp:
                raw = resp.read().decode("utf-8")
                return json.loads(raw) if raw else None
        except urllib.error.HTTPError as e:
            err = e.read().decode("utf-8", errors="replace")
            raise RuntimeError(f"HTTP {e.code} {url}: {err[:800]}") from e
        except Exception as e:
            last_err = e
            time.sleep(1.5 * (attempt + 1))
    raise RuntimeError(f"request failed after {retries} tries: {url}: {last_err}")


def login() -> str:
    if not USER or not PASS:
        raise SystemExit("Set KAOYAN_ADMIN_USER and KAOYAN_ADMIN_PASS")
    res = http_json(
        "POST",
        f"{BASE}/api/identity/Login/LoginByUserNameAndPwd",
        {"userName": USER, "password": PASS},
    )
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


def normalize_section(sec: dict) -> dict:
    qs = []
    for qi, q in enumerate(sec.get("questions") or []):
        opts = [str(o).strip() for o in (q.get("options") or []) if str(o).strip()]
        qs.append(
            {
                "number": q.get("number") or qi + 1,
                "stem": q.get("stem") or "",
                "options": opts,
                "correctAnswer": int(q.get("correctAnswer", 0)),
                "explanation": q.get("explanation") or "",
                "sequenceNumber": q.get("sequenceNumber") or qi + 1,
            }
        )
    return {
        "sectionType": sec.get("sectionType") or "reading",
        "title": sec.get("title") or "",
        "passage": sec.get("passage") or "",
        "sequenceNumber": sec.get("sequenceNumber") or 0,
        "questions": qs,
    }


def pick_cloze(existing: dict | None) -> dict:
    if existing:
        for s in existing.get("sections") or []:
            st = (s.get("sectionType") or s.get("SectionType") or "").lower()
            if st == "cloze":
                cloze = normalize_section(s)
                cloze["sectionType"] = "cloze"
                cloze["title"] = cloze["title"] or "完形填空"
                cloze["sequenceNumber"] = 1
                return cloze
    return {
        "sectionType": "cloze",
        "title": "完形填空",
        "passage": "",
        "sequenceNumber": 1,
        "questions": [],
    }


def main() -> None:
    files = sorted(PARSED.glob("*-reading.json"))
    if ONLY:
        files = [f for f in files if ONLY in f.stem]
    if not files:
        raise SystemExit(f"No reading JSON in {PARSED}")

    token = login()
    papers = http_json("GET", f"{BASE}/api/kaoyan/Admin/GetAllPapers", token=token) or []
    by_key = {
        f"{p.get('year') or p.get('Year')}-{p.get('series') or p.get('Series')}": p
        for p in papers
    }

    done = skip = 0
    for path in files:
        data = json.loads(path.read_text(encoding="utf-8"))
        year, series = data["year"], data["series"]
        key = f"{year}-{series}"
        if data.get("errors"):
            print(f"[SKIP] {key} errors: {data['errors']}")
            skip += 1
            continue
        if not data.get("answersReady"):
            print(f"[SKIP] {key} answersReady=false")
            skip += 1
            continue

        existing = None
        paper = by_key.get(key)
        if paper:
            pid = paper.get("id") or paper.get("Id")
            existing = http_json(
                "GET",
                f"{BASE}/api/kaoyan/Admin/GetPaperFull?paperId={pid}",
                token=token,
            )

        cloze = pick_cloze(existing)
        readings = [normalize_section(s) for s in data.get("sections") or []]
        for i, sec in enumerate(readings):
            sec["sectionType"] = "reading"
            sec["title"] = sec["title"] or f"阅读 Text{i + 1}"
            sec["sequenceNumber"] = i + 2

        sections = [cloze, *readings]
        payload = {
            "year": year,
            "series": series,
            "title": data.get("title")
            or f"{year}年考研{'英语一' if series == 'eng1' else '英语二'}",
            "sections": sections,
        }
        rq = sum(len(s["questions"]) for s in readings)
        print(
            f"{'[DRY] ' if DRY else ''}{key} cloze_q={len(cloze['questions'])} "
            f"reading_passages={len(readings)} reading_q={rq}"
        )
        if DRY:
            done += 1
            continue
        http_json("POST", f"{BASE}/api/kaoyan/Admin/ImportPaper", payload, token=token)
        print(f"  imported {key}")
        done += 1

    print(f"Done. imported/dry={done} skipped={skip}")


if __name__ == "__main__":
    main()
