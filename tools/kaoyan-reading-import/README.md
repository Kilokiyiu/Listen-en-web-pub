# 考研英语阅读理解导入

1. PDF 放在 `英语资源/考研英语/{年份}/阅读理解/`（`…第N篇真题及答案解析.pdf`）
2. 解析：

```bash
python tools/kaoyan-reading-import/parse_reading_pdf.py
```

产出：`英语资源/parsed/kaoyan/{year}-{eng1|eng2}-reading.json`（每套 4 个阅读分区）

3. 上传（保留已有完形）：

```powershell
$env:KAOYAN_ADMIN_USER="admin"
$env:KAOYAN_ADMIN_PASS="..."
$env:KAOYAN_DRY_RUN="1"
python tools/kaoyan-reading-import/upload_reading.py

$env:KAOYAN_DRY_RUN="0"
python tools/kaoyan-reading-import/upload_reading.py
```

可选：`KAOYAN_ONLY=2024-eng1`
