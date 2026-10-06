#!/usr/bin/env python3
import json
import os
import urllib.request

API = os.environ.get("WORD_API_BASE", "https://your-domain.com/api/word").rstrip("/")
ID = os.environ.get("IDENTITY_API_BASE", "https://your-domain.com/api/identity").rstrip("/")
UA = (
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
    "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36"
)


def req(method, url, body=None, token=None):
    data = None if body is None else json.dumps(body).encode()
    h = {
        "Content-Type": "application/json",
        "Accept": "application/json",
        "User-Agent": UA,
        "Origin": "https://your-domain.com",
        "Referer": "https://your-domain.com/",
    }
    if token:
        h["Authorization"] = f"Bearer {token}"
    r = urllib.request.Request(url, data=data, method=method, headers=h)
    with urllib.request.urlopen(r, timeout=60) as resp:
        raw = resp.read().decode()
        return json.loads(raw) if raw else {}


def main():
    login = req(
        "POST",
        f"{ID}/Login/LoginByUserNameAndPwd",
        {
            "userName": os.environ["ADMIN_USER"],
            "password": os.environ["ADMIN_PASSWORD"],
        },
    )
    token = login["token"]
    packs = req("GET", f"{API}/Admin/WordPacks", token=token).get("data") or []
    print("before:")
    for p in packs:
        print(p["code"], p["isPublished"], p["name"], "|", (p.get("description") or "")[:80])

    meta = {
        "cet4-core": {
            "name": "四级高频词",
            "category": "cet4",
            "description": "大学英语四级核心词汇",
            "sortOrder": 0,
        },
        "cet6-core": {
            "name": "六级高频词",
            "category": "cet6",
            "description": "大学英语六级核心词汇",
            "sortOrder": 1,
        },
        "kaoyan-core": {
            "name": "考研英语考纲高频词",
            "category": "kaoyan",
            "description": "考研英语考纲高频词",
            "sortOrder": 2,
            "publish": True,
        },
        "kaoyan-extra": {
            "name": "考研英语超纲词",
            "category": "kaoyan",
            "description": "考研英语超纲词",
            "sortOrder": 3,
            "publish": True,
        },
    }

    for p in packs:
        code = p["code"]
        m = meta.get(code, {})
        body = {
            "name": m.get("name", p["name"]),
            "category": m.get("category", p["category"]),
            "description": m.get("description", ""),
            "sortOrder": m.get("sortOrder", p.get("sortOrder") or 0),
        }
        req("PUT", f"{API}/Admin/WordPacks/{p['id']}", body, token)
        # 只清理文案，默认保持已发布状态；若明确配置 publish 则对齐
        if "publish" in m:
            want_pub = bool(m["publish"])
            if bool(p["isPublished"]) != want_pub:
                req("POST", f"{API}/Admin/WordPacks/{p['id']}/publish", None, token)
                print("toggled", code, "->", want_pub)
            else:
                print("updated", code)
        else:
            print("updated", code)

    packs2 = req("GET", f"{API}/Admin/WordPacks", token=token).get("data") or []
    print("after:")
    for p in packs2:
        print(p["code"], "pub=" + str(p["isPublished"]), p["name"], "|", p.get("description"))
    public = req("GET", f"{API}/word-packs")
    print("public:")
    for p in public if isinstance(public, list) else []:
        print(" ", p.get("code"), p.get("name"), "|", p.get("description"))


if __name__ == "__main__":
    main()
