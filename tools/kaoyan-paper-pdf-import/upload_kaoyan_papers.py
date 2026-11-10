# -*- coding: utf-8 -*-
"""
Upload 考研英语整卷真题/解析 PDF to KaoyanService.

Source layout:
  英语资源/整卷资源/考研英语/考研英一/{真题|解析}/*.pdf
  英语资源/整卷资源/考研英语/考研英二/{真题|解析}/*.pdf

Env:
  KAOYAN_API_BASE    default https://your-domain.com
  KAOYAN_ADMIN_USER / KAOYAN_ADMIN_PASS  required unless KAOYAN_DRY_RUN=1
  KAOYAN_DRY_RUN     1 = match only
  KAOYAN_ONLY        optional filter e.g. 2024 / eng1 / 2024-eng1
  KAOYAN_MAX_MB      skip files larger than N MB (0 = no limit)
"""
from __future__ import annotations

import json
import os
import re
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / "英语资源" / "整卷资源" / "考研英语"

BASE = os.environ.get("KAOYAN_API_BASE", "https://your-domain.com").rstrip("/")
USER = os.environ.get("KAOYAN_ADMIN_USER", "")
PASS = os.environ.get("KAOYAN_ADMIN_PASS", "")
ONLY = os.environ.get("KAOYAN_ONLY", "").strip().lower()
DRY = os.environ.get("KAOYAN_DRY_RUN", "0") == "1"
MAX_MB = float(os.environ.get("KAOYAN_MAX_MB", "0") or "0")

UA = (
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
    "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36"
)

SERIES_DIRS = {
    "eng1": SRC / "考研英一",
    "eng2": SRC / "考研英二",
}

YEAR_RE = re.compile(r"(20\d{2})")


def log(msg: str) -> None:
    print(msg, flush=True)


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
        raise SystemExit("Set KAOYAN_ADMIN_USER and KAOYAN_ADMIN_PASS")
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


def get_all_papers(token: str) -> list[dict]:
    return http_json("GET", f"{BASE}/api/kaoyan/Admin/GetAllPapers", token=token) or []


def extract_year(name: str) -> int | None:
    m = YEAR_RE.search(name)
    return int(m.group(1)) if m else None


def is_answer_name(name: str) -> bool:
    n = name.lower()
    return any(k in name for k in ("解析", "答案", "详解")) or n.endswith("_ans.pdf")


def is_skip_name(name: str) -> bool:
    return any(k in name for k in ("答题卡", "答题纸", "空白"))


def score_file(path: Path, kind: str) -> int:
    if path.suffix.lower() != ".pdf":
        return -1
    name = path.name
    if is_skip_name(name):
        return -1
    is_ans = is_answer_name(name)
    if kind == "paper" and is_ans:
        return -1
    if kind == "answer" and not is_ans:
        return -1
    score = 10
    if "真题" in name:
        score += 20
    if "可复制" in name or "高清" in name:
        score += 5
    # prefer smaller noise-free names slightly
    score -= min(len(name) // 40, 5)
    return score


def pick_best(files: list[Path], kind: str) -> Path | None:
    best = None
    best_score = -1
    for p in files:
        s = score_file(p, kind)
        if s > best_score:
            best_score = s
            best = p
    return best


def find_local(series: str, year: int) -> tuple[Path | None, Path | None]:
    base = SERIES_DIRS.get(series)
    if not base or not base.is_dir():
        return None, None

    paper_dirs = [base / "真题", base]
    answer_dirs = [base / "解析", base]

    papers: list[Path] = []
    answers: list[Path] = []
    for d in paper_dirs:
        if not d.is_dir():
            continue
        for p in d.glob("*.pdf"):
            if extract_year(p.name) == year:
                papers.append(p)
    for d in answer_dirs:
        if not d.is_dir():
            continue
        for p in d.glob("*.pdf"):
            if extract_year(p.name) == year:
                answers.append(p)

    # de-dupe by resolved path
    papers = list({p.resolve(): p for p in papers}.values())
    answers = list({p.resolve(): p for p in answers}.values())
    return pick_best(papers, "paper"), pick_best(answers, "answer")


def upload_pdf(token: str, paper_id: str, document_type: str, path: Path, retries: int = 3) -> dict:
    boundary = "----ListenEaseKaoyanBoundary7MA4YWxkTrZu0gW"
    with path.open("rb") as f:
        file_bytes = f.read()
    parts = []
    for k, v in (("paperId", paper_id), ("documentType", document_type)):
        parts.append(
            (
                f"--{boundary}\r\n"
                f'Content-Disposition: form-data; name="{k}"\r\n\r\n'
                f"{v}\r\n"
            ).encode("utf-8")
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
    url = f"{BASE}/api/kaoyan/Admin/UploadPaperDocument"
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
            try:
                with urllib.request.urlopen(req, timeout=(120, 600)) as resp:
                    raw = resp.read().decode("utf-8")
                    return json.loads(raw) if raw else {}
            except TypeError:
                with urllib.request.urlopen(req, timeout=600) as resp:
                    raw = resp.read().decode("utf-8")
                    return json.loads(raw) if raw else {}
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


def paper_key(p: dict) -> str:
    series = (p.get("series") or p.get("Series") or "").lower()
    year = p.get("year") if p.get("year") is not None else p.get("Year")
    return f"{year}-{series}"


def main() -> int:
    if not SRC.is_dir():
        print(f"Source not found: {SRC}", file=sys.stderr)
        return 1

    if DRY:
        # still need papers from API for matching, unless we invent keys from folders
        pass

    if not DRY and (not USER or not PASS):
        print("Set KAOYAN_ADMIN_USER and KAOYAN_ADMIN_PASS", file=sys.stderr)
        return 1

    token = "" if DRY else login()
    # Dry-run can still list remote papers if credentials exist
    if DRY and USER and PASS:
        token = login()
    elif DRY:
        log("DRY RUN without credentials: matching local files by year/series only")
        # synthesize jobs from local folders
        jobs = []
        for series, base in SERIES_DIRS.items():
            years = set()
            for sub in ("真题", "解析"):
                d = base / sub
                if not d.is_dir():
                    continue
                for p in d.glob("*.pdf"):
                    y = extract_year(p.name)
                    if y:
                        years.add(y)
            for y in sorted(years, reverse=True):
                key = f"{y}-{series}"
                if ONLY and ONLY not in key and ONLY not in series and ONLY not in str(y):
                    continue
                paper, answer = find_local(series, y)
                jobs.append(
                    {
                        "id": None,
                        "title": f"{y}年考研{'英语一' if series == 'eng1' else '英语二'}",
                        "key": key,
                        "paper": paper,
                        "answer": answer,
                    }
                )
        log(f"dry={DRY} matched_local={len(jobs)}")
        for j in jobs:
            p = f"{j['paper'].name} ({j['paper'].stat().st_size/1e6:.1f}MB)" if j["paper"] else "-"
            a = f"{j['answer'].name} ({j['answer'].stat().st_size/1e6:.1f}MB)" if j["answer"] else "-"
            flag = "OK" if j["paper"] and j["answer"] else "PARTIAL"
            log(f"[{flag}] {j['key']} | {j['title']}")
            log(f"       paper <- {p}")
            log(f"       answer<- {a}")
        log("DRY RUN done (no upload)")
        return 0

    remote = get_all_papers(token)
    jobs = []
    for p in remote:
        series = (p.get("series") or p.get("Series") or "").lower()
        year = p.get("year") if p.get("year") is not None else p.get("Year")
        if series not in ("eng1", "eng2") or year is None:
            continue
        key = f"{year}-{series}"
        title = p.get("title") or p.get("Title") or key
        if ONLY and ONLY not in key and ONLY not in str(title).lower() and ONLY not in series:
            continue
        paper, answer = find_local(series, int(year))
        jobs.append(
            {
                "id": p.get("id") or p.get("Id"),
                "title": title,
                "key": key,
                "paper": paper,
                "answer": answer,
                "has_paper": bool(p.get("hasPaper") or p.get("HasPaper") or p.get("paperFileUrl") or p.get("PaperFileUrl")),
                "has_answer": bool(p.get("hasAnswer") or p.get("HasAnswer") or p.get("answerFileUrl") or p.get("AnswerFileUrl")),
            }
        )

    log(f"dry={DRY} matched_papers={len(jobs)}")
    ok_both = sum(1 for j in jobs if j["paper"] and j["answer"])
    miss = [j for j in jobs if not j["paper"] or not j["answer"]]
    log(f"with both={ok_both} incomplete={len(miss)}")

    for j in jobs:
        p = f"{j['paper'].name} ({j['paper'].stat().st_size/1e6:.1f}MB)" if j["paper"] else "-"
        a = f"{j['answer'].name} ({j['answer'].stat().st_size/1e6:.1f}MB)" if j["answer"] else "-"
        flag = "OK" if j["paper"] and j["answer"] else "PARTIAL"
        log(f"[{flag}] {j['key']} | {j['title']}")
        log(f"       paper <- {p}")
        log(f"       answer<- {a}")

    if DRY:
        log("DRY RUN done (no upload)")
        return 0

    uploaded = 0
    errors = 0
    skipped = 0
    for j in jobs:
        pid = j["id"]
        for kind, path in (("paper", j["paper"]), ("answer", j["answer"])):
            if not path:
                continue
            mb = path.stat().st_size / (1024 * 1024)
            if MAX_MB > 0 and mb > MAX_MB:
                log(f"SKIP {kind} {j['key']} {path.name} ({mb:.1f}MB > {MAX_MB}MB)")
                skipped += 1
                continue
            try:
                res = upload_pdf(token, str(pid), kind, path)
                log(f"UP {kind} {j['key']} -> {res.get('url')} ({mb:.1f}MB)")
                uploaded += 1
            except Exception as e:
                log(f"ERR {kind} {j['key']}: {e}")
                errors += 1

    log(f"done uploads={uploaded} errors={errors} skipped={skipped}")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
