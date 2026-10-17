# -*- coding: utf-8 -*-
"""
解析 英语资源/考研英语/{year}/阅读理解/*.pdf
→ 英语资源/parsed/kaoyan/{year}-{eng1|eng2}-reading.json

每套试卷 4 篇，每篇约 5 题（21–40）。产出 4 个 reading 分区。
"""
from __future__ import annotations

import json
import re
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / "英语资源" / "考研英语"
OUT = ROOT / "英语资源" / "parsed" / "kaoyan"

NAME_RE = re.compile(r"英语([一二]).*?(\d{4}).*?第\s*(\d)\s*篇")
FOOTER_RE = re.compile(r"(?m)^懒笔记\s*·.*$")
EXPLANATION_MAX = 3900

Q_BLOCK_RE = re.compile(
    r"(?m)^(\d{2})\.\s+(.+?)\n"
    r"\s*\[\s*A\s*\]\s*(.+?)\n"
    r"\s*\[\s*B\s*\]\s*(.+?)\n"
    r"\s*\[\s*C\s*\]\s*(.+?)\n"
    r"\s*\[\s*D\s*\]\s*(.+?)(?=\n【|\n\d{2}\.\s|\n[^\S\n]*[\u4e00-\u9fff].{8,}|\Z)",
    re.S,
)
ANSWER_RE = re.compile(r"【答案】\s*([A-D])\s*[（(]?([^）)\n]*)[）)]?")
JUDGE_RE = re.compile(r"【判型】\s*(.*?)(?=\n【|\Z)", re.S)
LOCATE_RE = re.compile(r"【定位】\s*(.*?)(?=\n【|\Z)", re.S)
OPTS_ANA_RE = re.compile(r"【选项】\s*(.*?)(?=\n【|\n\d{2}\.\s|\n[^\S\n]*[\u4e00-\u9fff].{8,}|\Z)", re.S)


def pdf_text(path: Path) -> str:
    from pypdf import PdfReader

    text = "\n".join((p.extract_text() or "") for p in PdfReader(str(path)).pages)
    text = text.replace("\xa0", " ").replace("\u3000", " ")
    text = FOOTER_RE.sub("", text)
    text = re.sub(r"[ \t]+\n", "\n", text)
    text = re.sub(r"\n{3,}", "\n\n", text)
    return text.strip()


def infer_meta(path: Path) -> tuple[int, str, int]:
    m = NAME_RE.search(path.stem)
    if not m:
        raise ValueError(f"cannot parse name: {path.name}")
    series = "eng1" if m.group(1) == "一" else "eng2"
    return int(m.group(2)), series, int(m.group(3))


def cjk_ratio(s: str) -> float:
    if not s:
        return 0.0
    cjk = len(re.findall(r"[\u4e00-\u9fff]", s))
    return cjk / max(len(s), 1)


def _norm(s: str) -> str:
    s = re.sub(r"[ \t]+", " ", (s or "").replace("\r", ""))
    lines = [ln.strip() for ln in s.split("\n") if ln.strip()]
    out: list[str] = []
    for ln in lines:
        if not out:
            out.append(ln)
            continue
        prev = out[-1]
        if re.match(r"^[A-D]\)\s", ln) or ln.startswith("【"):
            out.append(ln)
            continue
        if prev[-1].isascii() and prev[-1].isalnum() and ln[:1].isascii():
            out[-1] = f"{prev} {ln}"
        elif cjk_ratio(prev) > 0.3 and cjk_ratio(ln) > 0.3:
            out[-1] = f"{prev}{ln}"
        else:
            out[-1] = f"{prev} {ln}" if prev[-1].isascii() or ln[:1].isascii() else f"{prev}{ln}"
    return "\n".join(out).strip()


def extract_passage(text: str) -> str:
    # Cut directions header
    m = re.search(r"\(40\s*points?\)\.?\s*", text, re.I)
    body = text[m.end() :] if m else text
    # Stop before question section
    for marker in ("题目与逐题解析", "【答案】", "\n21.", "\n26.", "\n31.", "\n36."):
        idx = body.find(marker)
        if idx > 80:
            body = body[:idx]
            break

    paras: list[str] = []
    # Split by P1 / P2 markers
    parts = re.split(r"(?m)^(P\d+)\s+", body)
    # parts: [preamble, 'P1', content, 'P2', content, ...]
    i = 1
    while i + 1 < len(parts):
        _tag = parts[i]
        chunk = parts[i + 1]
        i += 2
        eng_lines: list[str] = []
        for ln in chunk.split("\n"):
            t = ln.strip()
            if not t:
                if eng_lines:
                    break
                continue
            if t.startswith("Text ") or t.startswith("题目"):
                break
            if cjk_ratio(t) > 0.28:
                break
            eng_lines.append(t)
        if eng_lines:
            para = re.sub(r"\s+", " ", " ".join(eng_lines)).strip()
            if para:
                paras.append(para)
    return "\n\n".join(paras).strip()


def letter_to_index(letter: str) -> int:
    return ord(letter.upper()) - 65


def extract_questions(text: str) -> list[dict]:
    ans_matches = list(ANSWER_RE.finditer(text))
    blocks = list(Q_BLOCK_RE.finditer(text))
    qs: list[dict] = []
    for i, m in enumerate(blocks):
        num = int(m.group(1))
        stem = re.sub(r"\s+", " ", m.group(2).strip())
        opts = [re.sub(r"\s+", " ", m.group(j).strip()) for j in range(3, 7)]
        # Pair with the nearest 【答案】 before this English stem
        preceding = [a for a in ans_matches if a.start() < m.start()]
        if preceding:
            am = preceding[-1]
            letter = am.group(1)
            ans_tag = (am.group(2) or "").strip()
            region_start = am.start()
        else:
            letter, ans_tag = "A", ""
            region_start = m.start()
        # Region ends at the next 【答案】 (belongs to following question)
        following = [a for a in ans_matches if a.start() > m.start()]
        region_end = following[0].start() if following else len(text)
        ana = text[region_start:region_end]

        parts = [f"【答案】[{letter}] {ans_tag}".strip()]
        jm = JUDGE_RE.search(ana)
        if jm:
            judge = _norm(jm.group(1))
            judge = re.split(r"\n?\d{2}\.\s", judge, maxsplit=1)[0].strip()
            judge = re.split(r"\n?\s*\[\s*A\s*\]", judge, maxsplit=1)[0].strip()
            if judge:
                parts.append(f"【判型】{judge}")
        lm = LOCATE_RE.search(ana)
        if lm:
            loc = _norm(lm.group(1))
            loc_lines = [x for x in loc.split("\n") if x]
            eng = [x for x in loc_lines if cjk_ratio(x) < 0.28]
            parts.append(f"【定位】{' '.join(eng) if eng else loc_lines[0]}")
        om = OPTS_ANA_RE.search(ana)
        if om:
            parts.append(f"【选项】{_norm(om.group(1))}")
        expl = "\n".join(parts)
        if len(expl) > EXPLANATION_MAX:
            parts2 = [p for p in parts if not p.startswith("【选项】")]
            expl = "\n".join(parts2)
            if len(expl) > EXPLANATION_MAX:
                expl = expl[: EXPLANATION_MAX - 1] + "…"

        qs.append(
            {
                "number": num,
                "stem": stem,
                "options": [f"{chr(65 + k)}) {opts[k]}" for k in range(4)],
                "correctAnswer": letter_to_index(letter),
                "explanation": expl,
                "sequenceNumber": i + 1,
            }
        )
    return qs


def parse_one_pdf(path: Path) -> dict:
    year, series, pidx = infer_meta(path)
    text = pdf_text(path)
    passage = extract_passage(text)
    questions = extract_questions(text)
    errors = []
    if len(questions) != 5:
        errors.append(f"q={len(questions)} (want 5)")
    if len(passage) < 200:
        errors.append(f"passage_len={len(passage)}")
    expected_start = {1: 21, 2: 26, 3: 31, 4: 36}.get(pidx)
    if questions and expected_start and questions[0]["number"] != expected_start:
        errors.append(f"first_q={questions[0]['number']} (want {expected_start})")

    return {
        "year": year,
        "series": series,
        "passageIndex": pidx,
        "sourceFile": str(path.relative_to(ROOT)).replace("\\", "/"),
        "errors": errors,
        "section": {
            "sectionType": "reading",
            "title": f"阅读 Text{pidx}",
            "passage": passage,
            "sequenceNumber": pidx + 1,  # cloze=1, texts 2..5
            "questions": questions,
        },
    }


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    files = sorted(SRC.glob("*/阅读理解/*.pdf"))
    if not files:
        raise SystemExit(f"No reading PDFs under {SRC}")

    by_paper: dict[str, list[dict]] = defaultdict(list)
    for path in files:
        try:
            item = parse_one_pdf(path)
        except Exception as e:
            print(f"[ERR] {path.name}: {e}")
            continue
        key = f"{item['year']}-{item['series']}"
        by_paper[key].append(item)
        flag = "ok" if not item["errors"] else "warn"
        print(
            f"[{flag}] {key} Text{item['passageIndex']} "
            f"q={len(item['section']['questions'])} "
            f"pass={len(item['section']['passage'])} {item['errors']}"
        )

    summary = []
    ok = 0
    for key in sorted(by_paper.keys()):
        items = sorted(by_paper[key], key=lambda x: x["passageIndex"])
        year, series = items[0]["year"], items[0]["series"]
        sections = [it["section"] for it in items]
        # renumber sequence 2..n
        for i, sec in enumerate(sections):
            sec["sequenceNumber"] = i + 2
        errors = []
        if len(sections) != 4:
            errors.append(f"passages={len(sections)}")
        for it in items:
            errors.extend([f"T{it['passageIndex']}:{e}" for e in it["errors"]])
        data = {
            "year": year,
            "series": series,
            "title": f"{year}年考研{'英语一' if series == 'eng1' else '英语二'}",
            "answersReady": not errors,
            "errors": errors,
            "sections": sections,
            "sources": [it["sourceFile"] for it in items],
        }
        out_name = f"{key}-reading.json"
        (OUT / out_name).write_text(
            json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8"
        )
        status = "ok" if not errors else "warn"
        if not errors:
            ok += 1
        qtotal = sum(len(s["questions"]) for s in sections)
        summary.append(
            {"file": out_name, "status": status, "q": qtotal, "errors": errors}
        )
        print(f"==> {out_name} q={qtotal} [{status}]")

    (OUT / "reading-summary.json").write_text(
        json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print(f"Done. {ok}/{len(by_paper)} papers clean. Out -> {OUT.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
