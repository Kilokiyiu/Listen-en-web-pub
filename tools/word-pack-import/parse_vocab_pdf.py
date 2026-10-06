#!/usr/bin/env python3
"""
从 英语资源/单词资源 下的 PDF / JSON 解析词条，导出可导入管理后台的 JSON。
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

try:
    from pypdf import PdfReader
except ImportError:
    print("需要 pypdf: pip install pypdf", file=sys.stderr)
    sys.exit(1)

ROOT = Path(__file__).resolve().parents[2]
DEFAULT_DIR = ROOT / "英语资源" / "单词资源"
OUT_DIR = Path(__file__).resolve().parent / "out"

# 六级乱序：word + 词性.释义（两列）
CET6_RE = re.compile(
    r"([A-Za-z][A-Za-z\-']{1,40})\s+"
    r"((?:[a-z]{1,6}\.)+[^\nA-Za-z][^\n]*?)"
    r"(?=(?:\s+[A-Za-z][A-Za-z\-']{1,40}\s+(?:[a-z]{1,6}\.)+)|\s*$)",
    re.MULTILINE,
)

# 四级带音标：序号 单词 音标 释义
CET4_LINE_RE = re.compile(
    r"(?m)^\s*(\d+)\s+([A-Za-z][A-Za-z\-'.]*)\s+(\S*?)\s+((?:[a-z]+\.|adj\.|adv\.|prep\.|conj\.|num\.|pron\.|v\.aux\.|vt\.|vi\.|n\.|a\.).+)$",
)

# 考研榜单：# 单词 CEFR 释义 句频
KAOYAN_RE = re.compile(
    r"(?m)^\s*(\d+)\s+([A-Za-z][A-Za-z\-']*)\s+([A-C][12])\s+(.+?)\s+(\d+)\s*$",
)

POS_START = re.compile(
    r"^(?:n\.|v\.|vt\.|vi\.|a\.|adj\.|adv\.|prep\.|conj\.|pron\.|num\.|art\.|int\.|aux\.|modal\.|pl\.|sb\.|sth\.)",
    re.I,
)


def extract_text(pdf_path: Path) -> str:
    reader = PdfReader(str(pdf_path))
    return "\n".join((page.extract_text() or "") for page in reader.pages)


def finalize(found: dict[str, dict], keep_order: bool = False) -> list[dict]:
    items = list(found.values())
    if keep_order:
        items.sort(key=lambda x: x.get("rank") or 0)
    else:
        items.sort(key=lambda x: x["word"].lower())
        for i, item in enumerate(items, start=1):
            item["rank"] = i
    for item in items:
        w = item["word"]
        if w.isupper() or w.islower():
            item["word"] = w.lower()
        item["definition"] = re.sub(r"\s+", " ", item["definition"]).strip()[:1000]
        item["phonetic"] = (item.get("phonetic") or "")[:100]
        item["example"] = (item.get("example") or "")[:2000]
    return items


def parse_kaoyan(text: str) -> list[dict]:
    found: dict[str, dict] = {}
    for m in KAOYAN_RE.finditer(text.replace("\u3000", " ")):
        rank = int(m.group(1))
        word = m.group(2).strip()
        definition = m.group(4).strip()
        # 去掉行尾可能残留的省略号空格
        definition = re.sub(r"\s+", " ", definition)
        key = word.lower()
        if key in found:
            continue
        found[key] = {
            "word": word,
            "definition": definition,
            "phonetic": "",
            "example": "",
            "rank": rank,
        }
    return finalize(found, keep_order=True)


def parse_cet4_phonetic(text: str) -> list[dict]:
    """处理「序号 单词 注音 释义」表格型四级 PDF。"""
    # 合并被换行拆开的词（recommendati\non）
    text = re.sub(r"([A-Za-z])\n([a-z]{1,10})\b", r"\1\2", text)
    text = text.replace("\u3000", " ")

    found: dict[str, dict] = {}
    # 更宽松：按行，遇到「数字 英文词」开头
    line_re = re.compile(
        r"(?m)^\s*(\d+)\s+([A-Za-z][A-Za-z\-'.]*)\s+(.+)$"
    )
    for m in line_re.finditer(text):
        rank = int(m.group(1))
        word = m.group(2).strip().rstrip(".")
        rest = m.group(3).strip()
        if word.lower() in {"序号", "单词"} or not word.isascii():
            continue

        phonetic = ""
        definition = rest
        # 音标通常在词性前：...'xxx  n.释义 或 .rek...  n.
        pos_m = re.search(
            r"((?:n\.|v\.|vt\.|vi\.|a\.|adj\.|adv\.|prep\.|conj\.|pron\.|num\.|art\.|int\.|v\.aux\.|aux\.).*)$",
            rest,
            re.I,
        )
        if pos_m:
            definition = pos_m.group(1).strip()
            phonetic = rest[: pos_m.start()].strip()
        elif not POS_START.search(rest):
            # 无法识别词性则整段当释义
            definition = rest

        if len(word) < 2:
            continue
        key = word.lower()
        if key in {"the", "and", "for", "with", "from", "o'clock"} and rank > 2000:
            continue
        if key not in found:
            found[key] = {
                "word": word,
                "definition": definition,
                "phonetic": phonetic,
                "example": "",
                "rank": rank,
            }
    return finalize(found, keep_order=True)


def parse_cet6_columns(text: str) -> list[dict]:
    cleaned = text.replace("\u3000", " ")
    cleaned = re.sub(r"[ \t]+", " ", cleaned)
    found: dict[str, dict] = {}
    for m in CET6_RE.finditer(cleaned):
        word = m.group(1).strip()
        definition = m.group(2).strip()
        if len(word) < 2:
            continue
        if word.lower() in {"the", "and", "for", "with", "from"}:
            continue
        if "." not in definition:
            continue
        # 过滤音标误识别成单词（含非 ASCII / 过多撇号）
        if not re.fullmatch(r"[A-Za-z][A-Za-z\-']{0,40}", word):
            continue
        if word.count("'") > 1:
            continue
        key = word.lower()
        if key not in found:
            found[key] = {
                "word": word,
                "definition": definition,
                "phonetic": "",
                "example": "",
                "rank": 0,
            }
        elif len(definition) > len(found[key]["definition"]):
            found[key]["definition"] = definition
    return finalize(found, keep_order=False)


def detect_and_parse(path: Path, text: str) -> list[dict]:
    name = path.name
    # 考研榜单特征
    if "考研" in name or re.search(r"CEFR|完整总榜|句频", text[:2000]):
        items = parse_kaoyan(text)
        if items:
            return items
    # 四级带音标
    if "四级" in name or "注音" in text[:800] or re.search(r"序号\s+单词\s+注音", text[:800]):
        items = parse_cet4_phonetic(text)
        if len(items) >= 200:
            return items
    # 默认按六级双列
    items = parse_cet6_columns(text)
    if len(items) < 50 and "四级" in name:
        # 回退再试四级
        return parse_cet4_phonetic(text)
    return items


def parse_json_file(path: Path) -> list[dict]:
    data = json.loads(path.read_text(encoding="utf-8"))
    if isinstance(data, dict) and "entries" in data:
        data = data["entries"]
    if not isinstance(data, list):
        raise ValueError("JSON 需为数组或含 entries 数组")
    out = []
    for i, x in enumerate(data, start=1):
        word = (x.get("word") or x.get("Word") or "").strip()
        if not word:
            continue
        out.append(
            {
                "word": word,
                "definition": (x.get("definition") or x.get("meaning") or "")[:1000],
                "phonetic": x.get("phonetic") or "",
                "example": x.get("example") or "",
                "rank": int(x.get("rank") or i),
            }
        )
    return out


def guess_meta(name: str) -> tuple[str, str, str]:
    n = name.lower()
    if "四级" in name or "cet4" in n or "cet-4" in n:
        return "cet4-core", "四级高频词", "cet4"
    if "六级" in name or "cet6" in n or "cet-6" in n:
        return "cet6-core", "六级高频词", "cet6"
    if "超纲" in name:
        return "kaoyan-extra", "考研英语超纲词", "kaoyan"
    if "考研" in name or "kaoyan" in n:
        return "kaoyan-core", "考研英语考纲高频词", "kaoyan"
    stem = Path(name).stem
    return stem.lower().replace(" ", "-")[:40], stem[:40], "other"


def main() -> int:
    parser = argparse.ArgumentParser(description="解析单词 PDF/JSON → 导入用 JSON")
    parser.add_argument("--pdf", type=str, help="单个 PDF 路径")
    parser.add_argument("--json", type=str, help="已有 JSON 规范化")
    parser.add_argument("--dir", type=str, default=str(DEFAULT_DIR), help="扫描目录")
    parser.add_argument("--out", type=str, help="输出文件路径")
    args = parser.parse_args()

    OUT_DIR.mkdir(parents=True, exist_ok=True)
    targets: list[Path] = []

    if args.pdf:
        targets = [Path(args.pdf)]
    elif args.json:
        targets = [Path(args.json)]
    else:
        d = Path(args.dir)
        if not d.exists():
            print(f"目录不存在: {d}", file=sys.stderr)
            return 1
        targets = sorted(d.glob("*.pdf")) + sorted(d.glob("*.json"))

    if not targets:
        print("未找到 PDF/JSON", file=sys.stderr)
        return 1

    for path in targets:
        print(f"解析: {path.name}")
        if path.suffix.lower() == ".json":
            entries = parse_json_file(path)
        else:
            text = extract_text(path)
            entries = detect_and_parse(path, text)

        code, name, category = guess_meta(path.name)
        payload = {
            "code": code,
            "name": name,
            "category": category,
            "description": f"{name}，领取后可按间隔复习",
            "entries": entries,
        }

        out_path = Path(args.out) if args.out and len(targets) == 1 else OUT_DIR / f"{code}.json"
        out_path.parent.mkdir(parents=True, exist_ok=True)
        out_path.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")
        sample = entries[:2] if entries else []
        print(f"  → {out_path.name}  ({len(entries)} 词) sample={sample}")

    print("完成。")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
