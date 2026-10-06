# CET 听力 PDF → 在线题导入

1. PDF 放在仓库根目录 `英语资源/`（已 gitignore，不会进 GitHub）
2. 解析：

```bash
pip install pypdf
python tools/cet-listening-import/parse_cet_pdf.py
```

产出：`英语资源/parsed/cet4|cet6/*.json`

3. 录入线上（需要管理员账号）：

```bash
# PowerShell
$env:CET_ADMIN_USER="你的管理员用户名"
$env:CET_ADMIN_PASS="你的密码"
$env:CET_DRY_RUN="1"   # 先演练匹配
python tools/cet-listening-import/upload_cet_quiz.py

$env:CET_DRY_RUN="0"   # 真正写入
python tools/cet-listening-import/upload_cet_quiz.py
```

可选：`CET_ONLY=2026-06-1` 只导入某一套；`CET_API_BASE=https://your-domain.com`
