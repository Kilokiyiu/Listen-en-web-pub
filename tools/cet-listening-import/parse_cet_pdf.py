# -*- coding: utf-8 -*-
"""
Parse lazynote-style CET-4/6 listening PDF (真题及答案解析) into quiz JSON
for ListenService SaveQuizSections.

Output: 英语资源/parsed/{cet4|cet6}/{year}-{month}-{set}.json
"""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

from pypdf import PdfReader

ROOT = Path(__file__).resolve().parents[2]
PDF_ROOT = ROOT / "英语资源" / "四六级英语"
OUT_ROOT = ROOT / "英语资源" / "parsed"

FILENAME_RE = re.compile(
    r"英语(?P<level>四级|六级)(?P<year>\d{4})年(?P<month>\d{1,2})月第(?P<set>\d)套"
)

HAS_CJK = re.compile(r"[\u4e00-\u9fff]")
FOOTER_RE = re.compile(r"懒笔记|english-exam\.lazynote|--\s*\d+\s*of\s*\d+\s*--|PAPER\s*&\s*KEY", re.I)
ANSWER_RE = re.compile(r"【答案】\s*([A-D])")
STEM_RE = re.compile(
    r"【题目】\s*(.+?)(?=\n【翻译】|\nA\)|\n【答案】|\n懒笔记|\d{1,2}\.\s*A\))",
    re.S,
)
# Option block glued onto stem: "...Sweden? 22. A) Surveying..."
STEM_OPTS_CUT_RE = re.compile(r"\s+\d{1,2}\.\s*A\)\s+.*$", re.S)
STEM_OPT_A_CUT_RE = re.compile(r"\s+A\)\s+.*$", re.S)
EN_OPTS_BLOCK_RE = re.compile(
    r"(?m)^(?P<num>\d{1,2})\.\s*A\)\s*(?P<a>.+?)\s*\nB\)\s*(?P<b>.+?)\s*\nC\)\s*(?P<c>.+?)\s*\nD\)\s*(?P<d>.+?)(?=\n|$)"
)
QUESTIONS_BASED_RE = re.compile(
    r"Questions?\s+(\d+)\s+(?:and|to|-)\s+(\d+)\s+are based on",
    re.I,
)
SECTION_SPLIT_RE = re.compile(r"(?m)^Section\s+([ABC])\b")


def extract_pdf_text(path: Path) -> str:
    reader = PdfReader(str(path))
    parts = []
    for page in reader.pages:
        t = page.extract_text() or ""
        parts.append(t)
    text = "\n".join(parts)
    # normalize
    text = text.replace("\r\n", "\n").replace("\u00a0", " ")
    text = re.sub(r"[ \t]+\n", "\n", text)
    return text


def is_mostly_chinese(line: str) -> bool:
    s = line.strip()
    if not s:
        return False
    cjk = len(HAS_CJK.findall(s))
    latin = len(re.findall(r"[A-Za-z]", s))
    return cjk > 0 and cjk >= latin


def clean_stem(stem: str) -> str:
    """Keep only the English question prompt; drop glued options / footers."""
    stem = re.sub(r"\s+", " ", (stem or "")).strip()
    if not stem:
        return ""
    stem = STEM_OPTS_CUT_RE.sub("", stem)
    stem = STEM_OPT_A_CUT_RE.sub("", stem)
    stem = FOOTER_RE.split(stem)[0]
    stem = re.split(r"https?://", stem, maxsplit=1)[0]
    # drop trailing Chinese if any leaked in
    m = HAS_CJK.search(stem)
    if m:
        stem = stem[: m.start()]
    return stem.strip(" ·|/-").strip()


def clean_explanation(expl: str) -> str:
    """English-only explanation: strip Chinese translation and footers."""
    expl = re.sub(r"\s+", " ", (expl or "")).strip()
    if not expl:
        return ""
    expl = FOOTER_RE.split(expl)[0]
    expl = re.split(r"https?://", expl, maxsplit=1)[0]
    m = HAS_CJK.search(expl)
    if m:
        expl = expl[: m.start()]
    expl = expl.strip(" ·|/-").strip()
    if len(expl) > 400:
        expl = expl[:400].rstrip() + "…"
    return expl


def clean_transcript_lines(block: str) -> str:
    """Keep English dialogue/prose lines; drop Chinese translations and footers."""
    out = []
    for raw in block.split("\n"):
        line = raw.strip()
        if not line:
            if out and out[-1] != "":
                out.append("")
            continue
        if FOOTER_RE.search(line):
            continue
        if line.startswith("【"):
            continue
        # Questions header is often injected mid-transcript at page breaks
        if QUESTIONS_BASED_RE.search(line) or line.startswith("Questions"):
            continue
        if re.match(r"^Directions:", line, re.I):
            continue
        if re.match(r"^Part\s+", line, re.I):
            continue
        if re.match(r"^Section\s+[ABC]\b", line, re.I):
            continue
        # Drop exam option lines / analysis chrome that sometimes leak in
        if re.match(r"^\d{1,2}\.\s*[A-D]\)", line):
            continue
        if re.match(r"^[A-D]\)\s+", line):
            continue
        if "借用原文" in line or "借⽤原⽂" in line or line.startswith("【"):
            continue
        if is_mostly_chinese(line):
            continue
        # Mixed Chinese analysis with embedded English quotes — drop if CJK-heavy
        cjk = len(HAS_CJK.findall(line))
        if cjk >= 8 and cjk >= len(re.findall(r"[A-Za-z]", line)) * 0.35:
            continue
        # strip leading "1 " / "2 " paragraph numbers from transcript
        line = re.sub(r"^\d{1,2}\s+", "", line)
        if re.match(r"^https?://", line, re.I):
            continue
        out.append(line)
    # merge soft wraps: if line doesn't look like new speaker and previous exists
    merged = []
    speaker = re.compile(r"^(?:M|W|Man|Woman|男|女)\s*[:：]", re.I)
    for line in out:
        if not line:
            if merged and merged[-1] != "":
                merged.append("")
            continue
        if speaker.match(line) or not merged or merged[-1] == "":
            merged.append(line)
        else:
            # continuation
            merged[-1] = f"{merged[-1]} {line}".strip()
    # collapse multiple blanks
    result = []
    for line in merged:
        if line == "":
            if result and result[-1] != "":
                result.append("")
        else:
            result.append(line)
    while result and result[-1] == "":
        result.pop()
    return "\n".join(result).strip()


def parse_meta_from_filename(name: str) -> dict | None:
    m = FILENAME_RE.search(name)
    if not m:
        return None
    level = "cet4" if m.group("level") == "四级" else "cet6"
    return {
        "level": level,
        "year": int(m.group("year")),
        "month": int(m.group("month")),
        "set": int(m.group("set")),
        "albumNameCn": (
            f"{m.group('year')}年{int(m.group('month'))}月大学英语"
            f"{'四级' if level == 'cet4' else '六级'}听力真题（第{m.group('set')}套）"
        ),
    }


def extract_questions(text: str) -> list[dict]:
    """Extract questions with English options and correctAnswer index."""
    questions = []
    # Prefer English option blocks: "1. A) ...\nB) ..."
    en_opts = {}
    for m in EN_OPTS_BLOCK_RE.finditer(text):
        num = int(m.group("num"))
        opts = [
            f"A) {m.group('a').strip()}",
            f"B) {m.group('b').strip()}",
            f"C) {m.group('c').strip()}",
            f"D) {m.group('d').strip()}",
        ]
        # clean newlines inside options
        opts = [re.sub(r"\s+", " ", o) for o in opts]
        en_opts[num] = opts

    # Split by 【题目】
    parts = re.split(r"(?=【题目】)", text)
    q_index = 0
    for part in parts:
        if not part.startswith("【题目】"):
            continue
        q_index += 1
        stem_m = STEM_RE.search(part)
        stem = clean_stem(stem_m.group(1) if stem_m else "")
        ans_m = ANSWER_RE.search(part)
        if not ans_m:
            continue
        letter = ans_m.group(1).upper()
        correct = "ABCD".index(letter)

        # Try to find question number from following "N. A)" block near this stem
        # Use sequential q_index matching en_opts keys when sorted
        options = en_opts.get(q_index)
        if not options:
            # fallback: Chinese options under 【题目】 (less ideal)
            cn = re.findall(r"([A-D])\)\s*([^\n]+)", part.split("【答案】")[0])
            if len(cn) >= 4:
                options = [f"{k}) {v.strip()}" for k, v in cn[:4]]
            else:
                options = [f"{c}) (missing option)" for c in "ABCD"]
        options = [clean_explanation(re.sub(r"\s+", " ", o)) or o for o in options]

        # explanation: prefer English 【定位】, never keep Chinese translation
        expl = ""
        loc = re.search(r"【定位】\s*(.+?)(?=\n【|\n懒笔记|$)", part, re.S)
        if loc:
            expl = clean_explanation(loc.group(1))

        questions.append(
            {
                "number": q_index,
                "stem": stem,
                "options": options,
                "correctAnswer": correct,
                "explanation": expl or f"Answer {letter}",
            }
        )
    return questions


def scrub_parsed_doc(doc: dict) -> dict:
    """Post-process already-parsed JSON: clean stems/explanations in place."""

    def scrub_q(q: dict) -> None:
        q["stem"] = clean_stem(q.get("stem") or "")
        letter = (
            "ABCD"[q["correctAnswer"]]
            if isinstance(q.get("correctAnswer"), int) and 0 <= q["correctAnswer"] < 4
            else "?"
        )
        cleaned = clean_explanation(q.get("explanation") or "")
        q["explanation"] = cleaned or f"Answer {letter}"
        q["options"] = [clean_explanation(o) or o for o in (q.get("options") or [])]

    for sec in doc.get("apiSections") or []:
        for q in sec.get("questions") or []:
            scrub_q(q)
        if sec.get("transcript"):
            sec["transcript"] = clean_transcript_lines(sec.get("transcript") or "")
        elif "transcript" in sec:
            sec["transcript"] = ""
    for sec in doc.get("sections") or []:
        for q in sec.get("questions") or []:
            scrub_q(q)
        if sec.get("transcript"):
            sec["transcript"] = clean_transcript_lines(sec.get("transcript") or "")
    return doc


def scrub_all_parsed() -> int:
    """Rewrite all parsed JSON with cleaned stems/explanations. Returns file count."""
    n = 0
    for path in sorted(OUT_ROOT.glob("cet*/*.json")):
        doc = json.loads(path.read_text(encoding="utf-8"))
        scrub_parsed_doc(doc)
        path.write_text(json.dumps(doc, ensure_ascii=False, indent=2), encoding="utf-8")
        n += 1
    return n


# Restart of a numbered transcript paragraph / speaker turn after analysis.
_TRANSCRIPT_RESTART_RE = re.compile(
    r"(?m)^(?P<num>\d{1,2})\s+(?:(?:M|W|Man|Woman)\s*[:：]|[A-Za-z])",
    re.I,
)
_TIMU_RE = re.compile(r"【题目】")


def _first_timu_after(text: str, pos: int) -> int:
    m = _TIMU_RE.search(text, pos)
    return m.start() if m else len(text)


def _find_transcript_blocks(section_text: str) -> list[tuple[int, int]]:
    """
    Locate transcript blocks: each starts at a numbered '1 ...' English line
    and ends at the next 【题目】 (start of question analysis).
    """
    blocks: list[tuple[int, int]] = []
    for m in _TRANSCRIPT_RESTART_RE.finditer(section_text):
        if int(m.group("num")) != 1:
            continue
        start = m.start()
        if blocks and start < blocks[-1][1]:
            continue
        nl = section_text.find("\n", start)
        line = section_text[start: nl if nl >= 0 else start + 80]
        if re.match(r"^\d{1,2}\s+[A-D]\)", line):
            continue
        end = _first_timu_after(section_text, start)
        if end - start < 40:
            continue
        blocks.append((start, end))
    return blocks


def _associate_marker_to_block(
    marker_pos: int, blocks: list[tuple[int, int]]
) -> tuple[int, int] | None:
    """
    Map a 'Questions X-Y' marker to its transcript block.

    The marker may appear:
      - at the start of the block (before paragraph 1),
      - mid-transcript (page-break header),
      - after the transcript, mid-analysis (after first 【答案】).
    """
    if not blocks:
        return None
    for s, e in blocks:
        if s <= marker_pos < e:
            return (s, e)
    before = [(s, e) for s, e in blocks if e <= marker_pos]
    if before:
        return before[-1]
    after = [(s, e) for s, e in blocks if s >= marker_pos]
    if after:
        return after[0]
    return None


def split_passages(section_text: str, group_name: str) -> list[dict]:
    """
    Split one Section into passages by 'Questions X and Y are based on'.

    Lazynote PDFs often inject the Questions header mid-transcript (page break)
    or even mid-analysis. Transcripts are recovered as numbered '1 ...' blocks
    that run until 【题目】, then matched to each Questions marker by proximity.
    """
    markers = list(QUESTIONS_BASED_RE.finditer(section_text))
    if not markers:
        qs = extract_questions(section_text)
        return [
            {
                "groupName": group_name,
                "title": f"{group_name} Passage 1",
                "transcript": clean_transcript_lines(section_text),
                "questions": qs,
            }
        ]

    blocks = _find_transcript_blocks(section_text)
    used_block_starts: set[int] = set()
    passages = []

    for i, m in enumerate(markers):
        start_q, end_q = int(m.group(1)), int(m.group(2))
        block = _associate_marker_to_block(m.start(), blocks)

        if block and block[0] not in used_block_starts:
            used_block_starts.add(block[0])
            transcript_src = section_text[block[0] : block[1]]
        elif block and block[0] in used_block_starts:
            te = _first_timu_after(section_text, m.start())
            transcript_src = section_text[m.start() : te]
        else:
            te = _first_timu_after(section_text, m.start())
            lookback = section_text[max(0, m.start() - 2500) : m.start()]
            ones = [
                x
                for x in _TRANSCRIPT_RESTART_RE.finditer(lookback)
                if int(x.group("num")) == 1
            ]
            if ones:
                abs_start = max(0, m.start() - 2500) + ones[-1].start()
                transcript_src = section_text[abs_start:te]
            else:
                transcript_src = section_text[m.start() : te]

        transcript = clean_transcript_lines(transcript_src)

        next_start = markers[i + 1].start() if i + 1 < len(markers) else len(section_text)
        if i + 1 < len(markers):
            next_block = _associate_marker_to_block(markers[i + 1].start(), blocks)
            if next_block and next_block[0] > m.start():
                next_start = min(next_start, next_block[0])

        q_start = m.start()
        if block and block[1] <= m.start():
            q_start = block[1]
        q_region = section_text[q_start:next_start]

        all_qs = extract_questions(q_region)
        qs = [q for q in all_qs if start_q <= q["number"] <= end_q]
        if not qs:
            qs = all_qs

        kind = {
            "Section A": "News Report",
            "Section B": "Long Conversation",
            "Section C": "Passage",
        }.get(group_name, "Passage")
        title = f"{kind} {i + 1}（{start_q}-{end_q}）"

        passages.append(
            {
                "groupName": group_name,
                "title": title,
                "transcript": transcript,
                "questions": qs,
            }
        )
    return passages



def parse_document(text: str, meta: dict) -> dict:
    # cut front matter before first Section A
    idx = text.find("Section A")
    if idx < 0:
        idx = 0
    body = text[idx:]

    # split sections
    parts = SECTION_SPLIT_RE.split(body)
    # parts: [before, 'A', contentA, 'B', contentB, 'C', contentC]
    sections_map = {}
    for i in range(1, len(parts), 2):
        letter = parts[i]
        content = parts[i + 1] if i + 1 < len(parts) else ""
        sections_map[f"Section {letter}"] = content

    passages = []
    for g in ("Section A", "Section B", "Section C"):
        if g not in sections_map:
            continue
        passages.extend(split_passages(sections_map[g], g))

    # Fix question numbers globally: re-extract all questions from full text as source of truth for counts
    all_qs = extract_questions(text)
    return {
        **meta,
        "questionCount": len(all_qs),
        "passageCount": len(passages),
        "sections": passages,
        "allQuestionsFlat": all_qs,  # debug
    }


def assign_questions_to_passages(doc: dict) -> dict:
    """
    Re-assign flat questions into passages by title range（q-q）.
    Falls back to sequential fill if ranges missing.
    """
    flat = doc.get("allQuestionsFlat") or []
    by_num = {q["number"]: q for q in flat}
    used = set()
    for sec in doc["sections"]:
        m = re.search(r"（(\d+)-(\d+)）", sec["title"])
        if m:
            a, b = int(m.group(1)), int(m.group(2))
            qs = [by_num[n] for n in range(a, b + 1) if n in by_num]
            for n in range(a, b + 1):
                used.add(n)
            sec["questions"] = qs
    # leftover sequential
    leftovers = [q for q in flat if q["number"] not in used]
    if leftovers:
        for sec in doc["sections"]:
            if not sec.get("questions"):
                # take next few
                take = leftovers[:2] if leftovers else []
                sec["questions"] = take
                leftovers = leftovers[len(take) :]
    # drop debug
    doc.pop("allQuestionsFlat", None)
    # build API payload sections
    seq = 1
    api_sections = []
    for sec in doc["sections"]:
        api_sections.append(
            {
                "groupName": sec["groupName"],
                "title": sec["title"],
                "transcript": sec.get("transcript") or "",
                "audioUrl": None,
                "sequenceNumber": seq,
                "questions": [
                    {
                        "number": q["number"],
                        "stem": q["stem"],
                        "options": q["options"],
                        "correctAnswer": q["correctAnswer"],
                        "explanation": q.get("explanation") or "",
                        "sequenceNumber": i + 1,
                    }
                    for i, q in enumerate(sec.get("questions") or [])
                ],
            }
        )
        seq += 1
    doc["apiSections"] = api_sections
    doc["questionCount"] = sum(len(s["questions"]) for s in api_sections)
    return doc


def process_pdf(path: Path) -> dict | None:
    meta = parse_meta_from_filename(path.name)
    if not meta:
        print(f"SKIP name: {path.name}", file=sys.stderr)
        return None
    text = extract_pdf_text(path)
    doc = parse_document(text, meta)
    doc = assign_questions_to_passages(doc)
    doc["sourcePdf"] = str(path.relative_to(ROOT)) if path.is_relative_to(ROOT) else str(path)
    return doc


def main():
    import argparse

    ap = argparse.ArgumentParser(description="Parse CET listening PDFs or scrub parsed JSON")
    ap.add_argument(
        "--scrub-only",
        action="store_true",
        help="Only clean existing parsed JSON (stems/explanations); skip PDF parse",
    )
    args = ap.parse_args()

    if args.scrub_only:
        n = scrub_all_parsed()
        print(f"Scrubbed {n} parsed JSON files under {OUT_ROOT}")
        return

    OUT_ROOT.mkdir(parents=True, exist_ok=True)
    pdfs = sorted(PDF_ROOT.rglob("*.pdf"))
    print(f"Found {len(pdfs)} PDFs under {PDF_ROOT}")
    summary = []
    for pdf in pdfs:
        try:
            doc = process_pdf(pdf)
            if not doc:
                continue
            scrub_parsed_doc(doc)
            out_dir = OUT_ROOT / doc["level"]
            out_dir.mkdir(parents=True, exist_ok=True)
            out_name = f"{doc['year']}-{doc['month']:02d}-{doc['set']}.json"
            out_path = out_dir / out_name
            # don't store huge raw
            save = {k: v for k, v in doc.items() if k != "allQuestionsFlat"}
            out_path.write_text(json.dumps(save, ensure_ascii=False, indent=2), encoding="utf-8")
            summary.append(
                {
                    "file": pdf.name,
                    "album": doc["albumNameCn"],
                    "passages": doc["passageCount"],
                    "questions": doc["questionCount"],
                    "out": str(out_path.relative_to(ROOT)),
                }
            )
            print(
                f"OK {doc['albumNameCn']}: passages={doc['passageCount']} questions={doc['questionCount']} -> {out_name}"
            )
        except Exception as e:
            print(f"FAIL {pdf.name}: {e}", file=sys.stderr)
            summary.append({"file": pdf.name, "error": str(e)})

    (OUT_ROOT / "summary.json").write_text(
        json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    ok = sum(1 for s in summary if "error" not in s)
    print(f"Done. {ok}/{len(summary)} parsed. Summary -> 英语资源/parsed/summary.json")


if __name__ == "__main__":
    main()
