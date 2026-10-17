# -*- coding: utf-8 -*-
"""
解析考研完形填空：
  - docx（真题）：原文 __1__…__20__ + 四选项
  - pdf（真题及答案解析）：【答案】+【线索】等

输入：英语资源/考研英语/{year}/完形填空/
输出：英语资源/parsed/kaoyan/{year}-{eng1|eng2}-cloze.json

也可手工覆盖答案：英语资源/考研英语/answers.json
  { "2024-eng1": ["B","C",...], ... }
"""
from __future__ import annotations

import html
import json
import re
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / "英语资源" / "考研英语"
OUT = ROOT / "英语资源" / "parsed" / "kaoyan"

BLANK_RE = re.compile(r"__\s*(\d+)\s*__")
DIR_END_RE = re.compile(
    r"\(10\s*points?\)\.?\s*|ANSWER\s+SHEET[^.\n]*\.?\s*",
    re.I,
)
Q_START_RE = re.compile(r"(?m)^\s*1\s*[\.、．]?\s*$")
OPT_RE = re.compile(
    r"(?m)^\s*(\d{1,2})\s*[\.、．]?\s*\n"
    r"\s*\[\s*A\s*\]\s*(.*?)\s*\n"
    r"\s*\[\s*B\s*\]\s*(.*?)\s*\n"
    r"\s*\[\s*C\s*\]\s*(.*?)\s*\n"
    r"\s*\[\s*D\s*\]\s*(.*?)(?=\n\s*\d{1,2}\s*[\.、．]?\s*\n|\Z)",
    re.S,
)
NAME_RE = re.compile(r"英语([一二])\s*(\d{4})")
FOOTER_RE = re.compile(r"(?m)^懒笔记\s*·.*$", re.M)
# 解析块：题号. + 【答案】... 直到下一题或文末
ANALYSIS_RE = re.compile(
    r"^(\d{1,2})\.\s*\n"
    r"【答案】\s*\[\s*([A-D])\s*\]\s*([^\n]*)\n"
    r"(.*?)(?=^\d{1,2}\.\s*$|\Z)",
    re.M | re.S,
)
CLUE_RE = re.compile(r"【线索】\s*(.*?)(?=\n【选项】|\n【验证】|\Z)", re.S)
OPTIONS_RE = re.compile(r"【选项】\s*(.*?)(?=\n【验证】|\Z)", re.S)
VERIFY_RE = re.compile(r"【验证】\s*(.*?)(?=\n【|\Z)", re.S)
EXPLANATION_MAX = 3900  # DB nvarchar(4000)


def docx_text(path: Path) -> str:
    with zipfile.ZipFile(path) as z:
        xml = z.read("word/document.xml").decode("utf-8")
    text = re.sub(r"</w:p>", "\n", xml)
    text = re.sub(r"<[^>]+>", "", text)
    text = html.unescape(text)
    text = text.replace("\xa0", " ").replace("\u3000", " ")
    text = re.sub(r"[ \t]+\n", "\n", text)
    text = re.sub(r"\n{3,}", "\n\n", text)
    return text.strip()


def pdf_text(path: Path) -> str:
    from pypdf import PdfReader

    reader = PdfReader(str(path))
    text = "\n".join((page.extract_text() or "") for page in reader.pages)
    text = text.replace("\xa0", " ").replace("\u3000", " ")
    text = FOOTER_RE.sub("", text)
    text = re.sub(r"[ \t]+\n", "\n", text)
    text = re.sub(r"\n{3,}", "\n\n", text)
    return text.strip()


def infer_meta(path: Path) -> tuple[int, str]:
    m = NAME_RE.search(path.stem)
    if m:
        series = "eng1" if m.group(1) == "一" else "eng2"
        return int(m.group(2)), series
    year = int(path.parent.parent.name)
    series = "eng1" if "一" in path.stem else "eng2"
    return year, series


def extract_passage(text: str) -> str:
    q = Q_START_RE.search(text)
    body = text[: q.start()] if q else text
    last = None
    for m in DIR_END_RE.finditer(body):
        last = m
    if last:
        body = body[last.end() :]
    else:
        m = BLANK_RE.search(body)
        if m:
            start = body.rfind("\n", 0, m.start())
            body = body[start + 1 :] if start >= 0 else body[m.start() :]

    body = body.strip()
    body = BLANK_RE.sub(lambda m: f"__{int(m.group(1))}__", body)
    body = re.sub(r"\s*\n\s*", " ", body)
    body = re.sub(r" {2,}", " ", body).strip()
    return body


def extract_questions(text: str) -> list[dict]:
    q = Q_START_RE.search(text)
    block = text[q.start() :] if q else text
    found: dict[int, dict] = {}
    for m in OPT_RE.finditer(block):
        num = int(m.group(1))
        opts = [re.sub(r"\s+", " ", m.group(i).strip()) for i in range(2, 6)]
        found[num] = {
            "number": num,
            "stem": "",
            "options": [f"{chr(65 + i)}) {opts[i]}" for i in range(4)],
            "correctAnswer": 0,
            "explanation": "",
            "sequenceNumber": num,
            "answerMissing": True,
        }
    return [found[i] for i in range(1, 21) if i in found]


def _norm_block(s: str) -> str:
    """Normalize PDF wraps: keep option/paragraph breaks, join mid-sentence wraps."""
    s = (s or "").replace("\r", "")
    lines = [re.sub(r"[ \t]+", " ", ln).strip() for ln in s.split("\n")]
    lines = [ln for ln in lines if ln]
    out: list[str] = []
    for ln in lines:
        is_opt = bool(re.match(r"^[A-D]\)\s", ln))
        is_tag = ln.startswith("【")
        if not out or is_opt or is_tag:
            out.append(ln)
            continue
        prev = out[-1]
        # New option explanation paragraph often restarts with the English word
        if re.match(r"^[A-Za-z]", ln) and (
            prev.endswith(("。", "；", "：", "！", "？", "✗", "✓"))
            or re.search(r"[✗✓]\S*$", prev)
        ):
            out.append(ln)
            continue
        if prev[-1].isascii() and prev[-1].isalnum() and ln[:1].isascii():
            out[-1] = f"{prev} {ln}"
        elif not prev[-1].isascii() and not ln[:1].isascii():
            out[-1] = f"{prev}{ln}"
        else:
            out[-1] = f"{prev} {ln}" if prev[-1].isascii() or ln[:1].isascii() else f"{prev}{ln}"
    return "\n".join(out).strip()


def _fit_explanation(parts: list[str], limit: int = EXPLANATION_MAX) -> str:
    """Prefer full text; if over limit, drop 验证 then trim 选项."""
    text = "\n".join(p for p in parts if p).strip()
    if len(text) <= limit:
        return text
    # Drop 验证 first
    parts2 = [p for p in parts if not p.startswith("【验证】")]
    text = "\n".join(p for p in parts2 if p).strip()
    if len(text) <= limit:
        return text
    # Trim 选项 block
    trimmed: list[str] = []
    for p in parts2:
        if p.startswith("【选项】") and len("\n".join(trimmed + [p])) > limit:
            room = limit - len("\n".join(trimmed)) - 2
            if room > 40:
                trimmed.append(p[: room - 1].rstrip() + "…")
            break
        trimmed.append(p)
    text = "\n".join(trimmed).strip()
    return text[:limit]


def extract_analysis(pdf_path: Path) -> dict[int, dict]:
    """Return {num: {letter, explanation}} from 真题及答案解析.pdf"""
    text = pdf_text(pdf_path)
    out: dict[int, dict] = {}
    for m in ANALYSIS_RE.finditer(text):
        num = int(m.group(1))
        letter = m.group(2).upper()
        ans_tail = re.sub(r"\s+", " ", m.group(3).strip())
        body = m.group(4)
        clue_m = CLUE_RE.search(body)
        opts_m = OPTIONS_RE.search(body)
        verify_m = VERIFY_RE.search(body)
        parts = [f"【答案】[{letter}] {ans_tail}".strip()]
        if clue_m:
            parts.append(f"【线索】{_norm_block(clue_m.group(1))}")
        if opts_m:
            parts.append(f"【选项】{_norm_block(opts_m.group(1))}")
        if verify_m:
            parts.append(f"【验证】{_norm_block(verify_m.group(1))}")
        out[num] = {
            "letter": letter,
            "explanation": _fit_explanation(parts),
        }
    return out


def find_analysis_pdf(docx_path: Path) -> Path | None:
    year, series = infer_meta(docx_path)
    label = "一" if series == "eng1" else "二"
    folder = docx_path.parent
    # Prefer 「真题及答案解析」
    for p in folder.glob("*.pdf"):
        if label in p.stem and str(infer_meta(p)[0]) == str(year):
            if "解析" in p.stem or "答案" in p.stem:
                return p
    for p in folder.glob("*.pdf"):
        if label in p.stem and str(year) in p.stem:
            return p
    return None


def load_answers_override() -> dict[str, list]:
    candidates = [
        SRC / "answers.json",
        OUT / "answers.json",
        Path(__file__).with_name("answers.json"),
    ]
    merged: dict[str, list] = {}
    for p in candidates:
        if not p.is_file():
            continue
        data = json.loads(p.read_text(encoding="utf-8"))
        if isinstance(data, dict):
            merged.update(data)
    for year_dir in SRC.glob("*/answers.json"):
        data = json.loads(year_dir.read_text(encoding="utf-8"))
        if isinstance(data, dict):
            merged.update(data)
    return merged


def letter_to_index(v) -> int:
    if isinstance(v, int):
        if 0 <= v <= 3:
            return v
        if 1 <= v <= 4:
            return v - 1
        raise ValueError(f"invalid answer index: {v}")
    s = str(v).strip().upper()
    if s and s[0] in "ABCD":
        return ord(s[0]) - 65
    raise ValueError(f"invalid answer: {v}")


def apply_override(questions: list[dict], key: str, answers_map: dict) -> bool:
    raw = answers_map.get(key)
    if not raw:
        return False
    if len(raw) < 20:
        raise ValueError(f"{key}: need 20 answers, got {len(raw)}")
    for i, q in enumerate(questions):
        q["correctAnswer"] = letter_to_index(raw[i])
        q["answerMissing"] = False
    return True


def apply_pdf_analysis(questions: list[dict], analysis: dict[int, dict]) -> int:
    n = 0
    for q in questions:
        info = analysis.get(q["number"])
        if not info:
            continue
        q["correctAnswer"] = letter_to_index(info["letter"])
        q["explanation"] = info["explanation"]
        q["answerMissing"] = False
        n += 1
    return n


def parse_one(docx_path: Path, answers_map: dict) -> dict:
    text = docx_text(docx_path)
    year, series = infer_meta(docx_path)
    passage = extract_passage(text)
    questions = extract_questions(text)
    blanks = sorted({int(m.group(1)) for m in BLANK_RE.finditer(passage)})
    key = f"{year}-{series}"

    pdf = find_analysis_pdf(docx_path)
    pdf_hits = 0
    if pdf:
        analysis = extract_analysis(pdf)
        pdf_hits = apply_pdf_analysis(questions, analysis)

    # Manual override wins
    overridden = apply_override(questions, key, answers_map) if questions else False

    errors = []
    if len(questions) != 20:
        errors.append(f"options={len(questions)} (want 20)")
    if blanks != list(range(1, 21)):
        errors.append(f"blanks={blanks}")
    if not passage:
        errors.append("empty passage")
    if pdf is None and not overridden:
        errors.append("no analysis pdf")
    elif pdf is not None and pdf_hits != 20 and not overridden:
        errors.append(f"pdf_answers={pdf_hits}")

    answers_ready = (
        len(questions) == 20
        and all(not q.get("answerMissing") for q in questions)
        and not any(
            e.startswith("options=") or e.startswith("blanks=") or e == "empty passage"
            for e in errors
        )
    )

    return {
        "year": year,
        "series": series,
        "title": f"{year}年考研{'英语一' if series == 'eng1' else '英语二'}",
        "sourceFile": str(docx_path.relative_to(ROOT)).replace("\\", "/"),
        "analysisFile": (
            str(pdf.relative_to(ROOT)).replace("\\", "/") if pdf else None
        ),
        "answersReady": answers_ready,
        "errors": errors,
        "sections": [
            {
                "sectionType": "cloze",
                "title": "完形填空",
                "passage": passage,
                "sequenceNumber": 1,
                "questions": [
                    {
                        "number": q["number"],
                        "stem": q["stem"],
                        "options": q["options"],
                        "correctAnswer": q["correctAnswer"],
                        "explanation": q["explanation"],
                        "sequenceNumber": q["sequenceNumber"],
                    }
                    for q in questions
                ],
            }
        ],
    }


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    answers_map = load_answers_override()
    files = sorted(SRC.glob("*/完形填空/*.docx"))
    files = [p for p in files if "解析" not in p.stem]
    if not files:
        raise SystemExit(f"No docx under {SRC}")

    summary = []
    ok = 0
    ans_ok = 0
    for path in files:
        data = parse_one(path, answers_map)
        qs = data["sections"][0]["questions"]
        out_name = f"{data['year']}-{data['series']}-cloze.json"
        (OUT / out_name).write_text(
            json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8"
        )
        status = "ok" if not data["errors"] else "warn"
        if not data["errors"]:
            ok += 1
        if data["answersReady"]:
            ans_ok += 1
        summary.append(
            {
                "file": out_name,
                "status": status,
                "answersReady": data["answersReady"],
                "errors": data["errors"],
                "q": len(qs),
            }
        )
        flag = "ANS" if data["answersReady"] else "NOANS"
        print(
            f"[{status}/{flag}] {out_name} q={len(qs)} "
            f"pdf={'yes' if data['analysisFile'] else 'no'} {data['errors']}"
        )

    (OUT / "summary.json").write_text(
        json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    answers_dump = {}
    for row in summary:
        key = row["file"].replace("-cloze.json", "")
        data = json.loads((OUT / row["file"]).read_text(encoding="utf-8"))
        if not data.get("answersReady"):
            continue
        answers_dump[key] = [
            chr(65 + q["correctAnswer"]) for q in data["sections"][0]["questions"]
        ]
    (OUT / "answers.extracted.json").write_text(
        json.dumps(answers_dump, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print(
        f"Done. {ok}/{len(files)} clean, {ans_ok}/{len(files)} with answers. "
        f"Out -> {OUT.relative_to(ROOT)}"
    )


if __name__ == "__main__":
    main()
