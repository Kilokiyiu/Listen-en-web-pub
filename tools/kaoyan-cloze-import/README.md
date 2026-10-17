# 考研英语完形填空导入

1. 材料放在 `英语资源/考研英语/{年份}/完形填空/`（已 gitignore）：
   - `…真题.docx`：带 `__1__` 空格的原文 + 四选项
   - `…真题及答案解析.pdf`：含 `【答案】` / `【线索】` 的解析版
2. 解析（docx + pdf 自动合并）：

```bash
python tools/kaoyan-cloze-import/parse_cloze_docx.py
```

产出：`英语资源/parsed/kaoyan/{year}-{eng1|eng2}-cloze.json`  
另有 `answers.extracted.json` 方便核对答案字母。

可选手工覆盖：`英语资源/考研英语/answers.json`（格式 `{"2024-eng1":["B","C",...]}`）。

3. 录入线上（管理员账号）：

```powershell
$env:KAOYAN_ADMIN_USER="你的管理员用户名"
$env:KAOYAN_ADMIN_PASS="你的密码"
$env:KAOYAN_DRY_RUN="1"
python tools/kaoyan-cloze-import/upload_cloze.py

$env:KAOYAN_DRY_RUN="0"
python tools/kaoyan-cloze-import/upload_cloze.py
```

可选：`KAOYAN_ONLY=2024-eng1`；`KAOYAN_API_BASE=https://your-domain.com`  
调试无答案导入：`KAOYAN_ALLOW_NO_ANSWERS=1`（判题会全错，勿用于生产）
