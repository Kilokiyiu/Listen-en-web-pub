# 官方词本导入工具

从本地 `英语资源/单词资源/`（PDF）解析词条，导入管理后台「官方词本」。

> `英语资源/` 已在 `.gitignore` 中，不会进 GitHub。  
> 本目录的 `out/` 解析产物也不应提交。

## 1. 解析 PDF → JSON

```bash
pip install pypdf
python tools/word-pack-import/parse_vocab_pdf.py
```

输出在 `tools/word-pack-import/out/*.json`。

## 2. 导入方式

### A. 管理后台（推荐先试）

1. 打开管理端 → **官方词本** → 新建词本（如编码 `cet4-core`）
2. 点 **词条** → **批量导入**
3. 打开对应 JSON，只复制 `entries` 数组粘贴导入
4. 检查抽样后点 **发布**

### B. 脚本上传

```bash
# Windows PowerShell
$env:ADMIN_TOKEN="你的管理员JWT"
$env:WORD_API_BASE="https://your-domain.com/api/word"   # 或本地 http://localhost:xxxx
python tools/word-pack-import/upload_word_pack.py tools/word-pack-import/out --publish
```

## 3. JSON 格式

```json
{
  "code": "cet4-core",
  "name": "四级高频词",
  "category": "cet4",
  "description": "四级核心词汇",
  "entries": [
    {
      "word": "abandon",
      "definition": "v. 放弃；遗弃",
      "phonetic": "",
      "example": "",
      "rank": 1
    }
  ]
}
```

PDF 文本抽取受排版影响，解析结果请人工抽查后再发布。
