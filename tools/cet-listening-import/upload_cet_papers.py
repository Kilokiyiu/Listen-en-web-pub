# -*- coding: utf-8 -*-
"""
Replace CET4/CET6 paper/answer PDFs on ListenEase with files under
英语资源/整卷资源/四六级/{四级|六级}/{YYYY.MM}/{真题PDF|答案解析}/

Does NOT touch online quiz sections.

Env:
  CET_API_BASE   default https://your-domain.com
  CET_ADMIN_USER / CET_ADMIN_PASS  required unless CET_DRY_RUN=1
  CET_DRY_RUN    1 = match only
  CET_ONLY       optional filter substring (album name or cet4/2024-06-1)
  CET_SCOPE      existing (default) = only albums that already have paper/answer URL
                 all = also upload for albums currently missing PDFs
"""
from __future__ import annotations

import json
import mimetypes
import os
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / "英语资源" / "整卷资源" / "四六级"

BASE = os.environ.get("CET_API_BASE", "https://your-domain.com").rstrip("/")
USER = os.environ.get("CET_ADMIN_USER", "")
PASS = os.environ.get("CET_ADMIN_PASS", "")
ONLY = os.environ.get("CET_ONLY", "").strip().lower()
DRY = os.environ.get("CET_DRY_RUN", "0") == "1"
SCOPE = os.environ.get("CET_SCOPE", "existing").strip().lower()
# Skip files larger than this (MB). Empty/0 = no limit.
MAX_MB = float(os.environ.get("CET_MAX_MB", "0") or "0")

UA = (
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
    "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36"
)


def http_json(method: str, url: str, body=None, token: str | None = None):
    data = None
    headers = {
        "Accept": "application/json, text/plain, */*",
        "User-Agent": UA,
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
        raise SystemExit("Set CET_ADMIN_USER and CET_ADMIN_PASS")
    res = http_json(
        "POST",
        f"{BASE}/api/identity/Login/LoginByUserNameAndPwd",
        {"userName": USER, "password": PASS},
    )
    if isinstance(res, str):
        return res
    if isinstance(res, dict):
        for k in ("token", "Token", "accessToken"):
            if isinstance(res.get(k), str):
                return res[k]
        data = res.get("data")
        if isinstance(data, dict):
            for k in ("token", "Token", "accessToken"):
                if isinstance(data.get(k), str):
                    return data[k]
        if isinstance(data, str):
            return data
    raise RuntimeError(f"Unexpected login response: {res}")


def album_name(a: dict) -> str:
    n = a.get("name") or a.get("Name")
    if isinstance(n, dict):
        return n.get("chinese") or n.get("Chinese") or n.get("english") or ""
    return str(n or "")


def parse_album(name: str, level: str) -> dict | None:
    m = re.search(r"(\d{4})\s*年\s*(\d{1,2})\s*月.*?第\s*(\d)\s*套", name)
    if not m:
        return None
    y, mo, s = int(m.group(1)), int(m.group(2)), int(m.group(3))
    return {
        "level": level,
        "year": y,
        "month": mo,
        "set": s,
        "folder": f"{y}.{mo:02d}",
        "key": f"{level}/{y}-{mo:02d}-{s}",
    }


def score_paper(path: Path, set_no: int) -> int:
    n = path.name
    nl = n.lower()
    if path.suffix.lower() != ".pdf":
        return -1
    # skip obvious answer files wrongly placed
    if any(k in n for k in ("解析", "答案", "详解", "速查")) or nl.endswith("_ans.pdf"):
        return -1
    score = 0
    patterns = [
        (rf"第\s*{set_no}\s*套", 50),
        (rf"第{set_no}套", 50),
        (rf"[（(]第{set_no}套[）)]", 50),
        (rf"[（(]卷\s*{set_no}[）)]", 45),
        (rf"卷\s*{set_no}", 40),
        (rf"_{set_no}\.pdf$", 55),
        (rf"-{set_no}\.pdf$", 50),
        (rf"全{set_no}套", 35),
        (rf"第{set_no}、", 20),
        (rf"第{set_no}-", 20),
    ]
    for pat, pts in patterns:
        if re.search(pat, n) or re.search(pat, nl):
            score = max(score, pts)
    ord_map = {1: "一", 2: "二", 3: "三"}
    if ord_map.get(set_no) and re.search(rf"[第卷]{ord_map[set_no]}", n):
        score = max(score, 45)
    if re.search(r"cet[46]_\d{4}_\d{2}_\d", nl):
        score += 20
    if "真题" in n or "试题" in n:
        score += 5
    return score


def score_answer(path: Path, set_no: int) -> int:
    n = path.name
    nl = n.lower()
    if path.suffix.lower() != ".pdf":
        return -1
    score = 0
    patterns = [
        (rf"第\s*{set_no}\s*套", 50),
        (rf"第{set_no}套", 50),
        (rf"[（(]第{set_no}套[）)]", 50),
        (rf"[（(]卷\s*{set_no}[）)]", 45),
        (rf"卷\s*{set_no}", 40),
        (rf"_{set_no}_ans\.pdf$", 70),
        (rf"_{set_no}\.pdf$", 40),
        (rf"全{set_no}套", 35),
        (rf"第{set_no}、", 20),
    ]
    for pat, pts in patterns:
        if re.search(pat, n) or re.search(pat, nl):
            score = max(score, pts)
    ord_map = {1: "一", 2: "二", 3: "三"}
    if ord_map.get(set_no) and re.search(rf"(第|卷){ord_map[set_no]}", n):
        score = max(score, 45)
    if "_ans" in nl or re.search(r"cet[46]_\d{4}_\d{2}_\d_ans", nl):
        score += 25
    if "速查" in n:
        score += 20
    if any(k in n for k in ("解析", "答案", "详解")):
        score += 10
    # avoid huge combined paper+answer packs when smaller解析 exists
    if "原卷" in n:
        score -= 30
    return score


def pick_best(files: list[Path], set_no: int, kind: str) -> Path | None:
    scorer = score_paper if kind == "paper" else score_answer
    ranked: list[tuple[int, int, Path]] = []
    for p in files:
        s = scorer(p, set_no)
        if s > 0:
            ranked.append((s, p.stat().st_size, p))
    if not ranked:
        return None
    # higher score first; then SMALLER file (upload-friendly)
    ranked.sort(key=lambda x: (-x[0], x[1]))
    return ranked[0][2]


def find_local(meta: dict) -> tuple[Path | None, Path | None, str]:
    level_dir = "四级" if meta["level"] == "cet4" else "六级"
    base = SRC / level_dir / meta["folder"]
    if not base.is_dir():
        return None, None, f"missing folder {base.relative_to(ROOT)}"

    paper_dirs = [base / "真题PDF", base / "真题Word", base]
    ans_dirs = [base / "答案解析", base]

    papers: list[Path] = []
    for d in paper_dirs:
        if d.is_dir():
            papers.extend(p for p in d.iterdir() if p.is_file())
    answers: list[Path] = []
    for d in ans_dirs:
        if d.is_dir():
            answers.extend(p for p in d.iterdir() if p.is_file())

    # dedupe by path
    papers = list({p.resolve(): p for p in papers}.values())
    answers = list({p.resolve(): p for p in answers}.values())

    paper = pick_best(papers, meta["set"], "paper")
    answer = pick_best(answers, meta["set"], "answer")
    note = ""
    if paper and paper.suffix.lower() != ".pdf":
        note = f"paper is {paper.suffix}, skip upload"
        paper = None
    return paper, answer, note


def log(msg: str) -> None:
    print(msg, flush=True)


def upload_pdf(token: str, album_id: str, document_type: str, path: Path, retries: int = 3) -> dict:
    import time

    boundary = "----ListenEaseBoundary7MA4YWxkTrZu0gW"
    with path.open("rb") as f:
        file_bytes = f.read()
    parts = []
    fields = {
        "albumId": album_id,
        "documentType": document_type,
    }
    for k, v in fields.items():
        parts.append(
            f"--{boundary}\r\n"
            f'Content-Disposition: form-data; name="{k}"\r\n\r\n'
            f"{v}\r\n".encode("utf-8")
        )
    safe_name = re.sub(r"[^\w.\-]+", "_", path.name)
    header = (
        f"--{boundary}\r\n"
        f'Content-Disposition: form-data; name="file"; filename="{safe_name}"\r\n'
        f"Content-Type: application/pdf\r\n\r\n"
    ).encode("utf-8")
    parts.append(header + file_bytes + b"\r\n")
    parts.append(f"--{boundary}--\r\n".encode("utf-8"))
    body = b"".join(parts)
    mb = len(file_bytes) / (1024 * 1024)
    url = f"{BASE}/api/listen/Admin/UploadAlbumDocument"
    last_err: Exception | None = None
    for attempt in range(1, retries + 1):
        req = urllib.request.Request(
            url,
            data=body,
            method="POST",
            headers={
                "Accept": "application/json",
                "User-Agent": UA,
                "Origin": BASE,
                "Referer": f"{BASE}/",
                "Authorization": f"Bearer {token}",
                "Content-Type": f"multipart/form-data; boundary={boundary}",
            },
        )
        try:
            # large PDFs need long timeout; nginx Admin location should be >= 600s
            with urllib.request.urlopen(req, timeout=(120, 600)) as resp:
                raw = resp.read().decode("utf-8")
                return json.loads(raw) if raw else {}
        except TypeError:
            # older Python: urlopen timeout is a single number
            try:
                with urllib.request.urlopen(req, timeout=600) as resp:
                    raw = resp.read().decode("utf-8")
                    return json.loads(raw) if raw else {}
            except Exception as e:
                last_err = e
        except urllib.error.HTTPError as e:
            err = e.read().decode("utf-8", errors="replace")
            last_err = RuntimeError(
                f"HTTP {e.code} upload {document_type} {path.name} ({mb:.1f}MB): {err[:800]}"
            )
            if e.code in (401, 403, 404):
                break
        except Exception as e:
            last_err = e
        log(f"  retry {attempt}/{retries} {document_type} {path.name} ({mb:.1f}MB): {last_err}")
        time.sleep(min(8, 2 * attempt))
    raise RuntimeError(str(last_err))


def main() -> int:
    if not SRC.is_dir():
        print(f"Source not found: {SRC}", file=sys.stderr)
        return 1

    cats = http_json("GET", f"{BASE}/api/listen/Listen/GetCategories")
    jobs = []
    for cat in cats:
        code = (cat.get("code") or cat.get("Code") or "").lower()
        if code not in ("cet4", "cet6"):
            continue
        cid = cat.get("id") or cat.get("Id")
        albums = http_json(
            "GET",
            f"{BASE}/api/listen/Listen/GetAlbumsByCategoryId?categoryId={cid}",
        )
        for a in albums or []:
            name = album_name(a)
            meta = parse_album(name, code)
            if not meta:
                continue
            if ONLY and ONLY not in meta["key"] and ONLY not in name.lower():
                continue
            paper_url = a.get("paperFileUrl") or a.get("PaperFileUrl")
            answer_url = a.get("answerFileUrl") or a.get("AnswerFileUrl")
            has_doc = bool(paper_url or answer_url)
            if SCOPE == "existing" and not has_doc:
                continue
            paper, answer, note = find_local(meta)
            jobs.append(
                {
                    "id": a.get("id") or a.get("Id"),
                    "name": name,
                    "meta": meta,
                    "has_doc": has_doc,
                    "paper": paper,
                    "answer": answer,
                    "note": note,
                    "old_paper": paper_url,
                    "old_answer": answer_url,
                }
            )

    log(f"scope={SCOPE} dry={DRY} matched_albums={len(jobs)}")
    ok_both = sum(1 for j in jobs if j["paper"] and j["answer"])
    ok_paper = sum(1 for j in jobs if j["paper"])
    ok_ans = sum(1 for j in jobs if j["answer"])
    miss = [j for j in jobs if not j["paper"] or not j["answer"]]
    log(f"with paper={ok_paper} with answer={ok_ans} with both={ok_both} incomplete={len(miss)}")

    for j in jobs:
        p = f"{j['paper'].name} ({j['paper'].stat().st_size/1e6:.1f}MB)" if j["paper"] else "-"
        a = f"{j['answer'].name} ({j['answer'].stat().st_size/1e6:.1f}MB)" if j["answer"] else "-"
        flag = "OK" if j["paper"] and j["answer"] else "PARTIAL"
        log(f"[{flag}] {j['meta']['key']} | {j['name']}")
        log(f"       paper <- {p}")
        log(f"       answer<- {a}" + (f" ({j['note']})" if j["note"] else ""))

    if DRY:
        log("DRY RUN done (no upload)")
        return 0

    token = login()
    uploaded = 0
    errors = 0
    skipped = 0
    for j in jobs:
        aid = j["id"]
        for kind, path in (("paper", j["paper"]), ("answer", j["answer"])):
            if not path:
                continue
            mb = path.stat().st_size / (1024 * 1024)
            if MAX_MB > 0 and mb > MAX_MB:
                log(f"SKIP {kind} {j['meta']['key']} {path.name} ({mb:.1f}MB > {MAX_MB}MB)")
                skipped += 1
                continue
            try:
                res = upload_pdf(token, aid, kind, path)
                log(f"UP {kind} {j['meta']['key']} -> {res.get('url')} ({mb:.1f}MB)")
                uploaded += 1
            except Exception as e:
                log(f"ERR {kind} {j['meta']['key']}: {e}")
                errors += 1

    log(f"done uploads={uploaded} errors={errors} skipped={skipped}")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
