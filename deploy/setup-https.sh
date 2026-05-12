#!/bin/bash
# HTTPS 部署脚本 - Listen-en-web-deploy
# 使用 Let's Encrypt 免费证书

set -e

DOMAIN="listenease.online"
EMAIL="your-email@example.com"  # 修改为你的邮箱（用于 Let's Encrypt 证书通知）
DEPLOY_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "========================================"
echo "  HTTPS 部署脚本 - Listen-en-web"
echo "========================================"
echo ""

# 1. 创建 certbot 目录
echo "[1/5] 创建证书目录..."
mkdir -p "$DEPLOY_DIR/certbot/conf"
mkdir -p "$DEPLOY_DIR/certbot/www"

# 2. 检查 .env 文件
echo "[2/5] 检查配置文件..."
if [ ! -f "$DEPLOY_DIR/.env" ]; then
    echo "错误: .env 文件不存在！"
    echo "请先创建 .env 文件，包含以下内容："
    echo "  SA_PASSWORD=你的SQL Server密码"
    echo "  JWT_KEY=你的JWT密钥（至少32个字符）"
    exit 1
fi

# 3. 启动基础服务（不带 Nginx）
echo "[3/5] 启动基础服务（数据库和后端API）..."
cd "$DEPLOY_DIR"

# 自动检测 docker compose 版本
if command -v docker-compose &> /dev/null; then
    COMPOSE_CMD="docker-compose"
elif docker compose version &> /dev/null; then
    COMPOSE_CMD="docker compose"
else
    echo "错误: 未找到 docker-compose 或 docker compose 命令"
    exit 1
fi
echo "使用: $COMPOSE_CMD"

$COMPOSE_CMD up -d sqlserver identity-service listen-service article-service

# 4. 等待服务启动
echo "[4/5] 等待服务启动..."
echo "  - 等待 SQL Server 启动..."
for i in {1..30}; do
    if docker exec listen_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$(grep SA_PASSWORD .env | cut -d'=' -f2)" -Q "SELECT 1" -C -No 2>/dev/null | grep -q "1"; then
        echo "  ✓ SQL Server 已就绪"
        break
    fi
    echo "  等待中... ($i/30)"
    sleep 2
done

# 5. 申请 SSL 证书
echo "[5/5] 申请 Let's Encrypt SSL 证书..."
echo "  域名: $DOMAIN"
echo "  邮箱: $EMAIL"
echo ""

# 创建临时的 nginx 配置用于 ACME 验证
cat > "$DEPLOY_DIR/nginx/temp-nginx.conf" << 'EOF'
worker_processes auto;
events { worker_connections 1024; }
http {
    server {
        listen 80;
        server_name DOMAIN_PLACEHOLDER;
        location /.well-known/acme-challenge/ {
            root /var/www/certbot;
        }
    }
}
EOF

sed -i "s/DOMAIN_PLACEHOLDER/$DOMAIN www.$DOMAIN/g" "$DEPLOY_DIR/nginx/temp-nginx.conf"

# 启动临时的 nginx 用于 ACME 验证
docker run -d --name temp_nginx \
    -v "$DEPLOY_DIR/nginx/temp-nginx.conf:/etc/nginx/nginx.conf:ro" \
    -v "$DEPLOY_DIR/certbot/www:/var/www/certbot:rw" \
    -p 80:80 \
    nginx:stable

sleep 3

# 申请证书
docker run --rm \
    -v "$DEPLOY_DIR/certbot/conf:/etc/letsencrypt" \
    -v "$DEPLOY_DIR/certbot/www:/var/www/certbot" \
    certbot/certbot certonly \
    --webroot \
    --webroot-path=/var/www/certbot \
    -d "$DOMAIN" \
    -d "www.$DOMAIN" \
    --email "$EMAIL" \
    --agree-tos \
    --no-eff-email \
    --force-renewal || true

# 清理临时容器
docker stop temp_nginx 2>/dev/null || true
docker rm temp_nginx 2>/dev/null || true
rm -f "$DEPLOY_DIR/nginx/temp-nginx.conf"

# 检查证书是否生成
if [ -d "$DEPLOY_DIR/certbot/conf/live/$DOMAIN" ]; then
    echo ""
    echo "✅ SSL 证书申请成功！"
    echo "  证书路径: $DEPLOY_DIR/certbot/conf/live/$DOMAIN"
else
    echo ""
    echo "⚠️  证书申请可能失败，请检查域名解析是否正确"
    echo "  确保以下记录已添加到 DNS:"
    echo "    - A 记录: $DOMAIN -> 你的服务器IP"
    echo "    - A 记录: www.$DOMAIN -> 你的服务器IP"
    echo ""
    echo "你可以在修复 DNS 后重新运行此脚本"
    exit 1
fi

# 6. 启动 Nginx
echo ""
echo "启动 Nginx..."
cd "$DEPLOY_DIR"
$COMPOSE_CMD up -d nginx certbot

echo ""
echo "========================================"
echo "  ✅ HTTPS 部署完成！"
echo "========================================"
echo ""
echo "访问地址:"
echo "  - https://$DOMAIN"
echo "  - https://www.$DOMAIN"
echo ""
echo "HTTP 会自动跳转到 HTTPS"
echo ""
echo "证书续期: Let's Encrypt 证书有效期 90 天"
echo "         certbot 会自动在后台续期（每天检查一次）"
echo ""
