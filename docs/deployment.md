// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Deployment Guide

This guide covers deploying the aspnet-spa-template to production environments.

## Table of Contents
- [Pre-Deployment Checklist](#pre-deployment-checklist)
- [Environment Configuration](#environment-configuration)
- [Docker Deployment](#docker-deployment)
- [Azure Deployment](#azure-deployment)
- [AWS Deployment](#aws-deployment)
- [IIS Deployment](#iis-deployment)
- [Linux/VM Deployment](#linuxvm-deployment)
- [Post-Deployment Verification](#post-deployment-verification)
- [Monitoring and Maintenance](#monitoring-and-maintenance)

## Pre-Deployment Checklist

Before deploying to production:

- [ ] All unit tests passing: `dotnet test`
- [ ] Code reviewed and approved
- [ ] Security scan completed
- [ ] Performance tested under load
- [ ] Database migrations tested
- [ ] Configuration reviewed (no dev secrets)
- [ ] SSL certificates configured
- [ ] Database backups scheduled
- [ ] Logging configured
- [ ] Monitoring alerts set up
- [ ] Disaster recovery plan documented
- [ ] Team trained on deployment process

## Environment Configuration

### Production appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db-server;Database=AspNetSpaTemplate;User ID=app_user;Password=secure_password;TrustServerCertificate=false;"
  },
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "/etc/ssl/certs/certificate.pfx",
          "Password": "${CERT_PASSWORD}"
        }
      }
    }
  },
  "AllowedHosts": "yourdomain.com",
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerMinute": 100
  }
}
```

### Environment Variables

```bash
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://0.0.0.0:443

# Database
ConnectionStrings__DefaultConnection=Server=db-server;...

# Logging
Logging__LogLevel__Default=Warning

# Security
CERT_PASSWORD=your-cert-password
```

## Docker Deployment

### Build Docker Image

```bash
# Build production image
docker build -f Dockerfile -t aspnet-spa-template:latest .

# Tag with registry
docker tag aspnet-spa-template:latest myregistry.azurecr.io/aspnet-spa-template:latest

# Push to registry
docker push myregistry.azurecr.io/aspnet-spa-template:latest
```

### Docker Compose Production

```yaml
version: '3.8'

services:
  app:
    image: myregistry.azurecr.io/aspnet-spa-template:latest
    ports:
      - "443:443"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: https://0.0.0.0:443
      ConnectionStrings__DefaultConnection: Server=db;Database=AspNetSpaTemplate;...
    volumes:
      - ./certs:/etc/ssl/certs:ro
    depends_on:
      - db
    restart: always
    healthcheck:
      test: ["CMD", "curl", "-f", "https://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  db:
    image: mcr.microsoft.com/mssql/server:latest
    environment:
      SA_PASSWORD: YourSecurePassword123!
      ACCEPT_EULA: Y
    volumes:
      - db_data:/var/opt/mssql
    restart: always

volumes:
  db_data:
```

### Deploy with Docker

```bash
# Deploy using docker-compose
docker-compose -f docker-compose.prod.yml up -d

# Check logs
docker logs container-name

# Stop services
docker-compose -f docker-compose.prod.yml down
```

## Azure Deployment

### Publish to Azure App Service

```bash
# Create resource group
az group create --name AspNetSpaGroup --location eastus

# Create App Service plan
az appservice plan create \
  --name AspNetSpaPlan \
  --resource-group AspNetSpaGroup \
  --sku B2 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group AspNetSpaGroup \
  --plan AspNetSpaPlan \
  --name aspnet-spa-app \
  --runtime "DOTNETCORE|10.0"

# Configure database connection
az webapp config appsettings set \
  --resource-group AspNetSpaGroup \
  --name aspnet-spa-app \
  --settings \
  ConnectionStrings__DefaultConnection="Server=aspnet-spa-db.database.windows.net;..."

# Deploy from local repository
az webapp up --name aspnet-spa-app --resource-group AspNetSpaGroup
```

### Azure SQL Database

```bash
# Create SQL Server
az sql server create \
  --resource-group AspNetSpaGroup \
  --name aspnet-spa-db-server \
  --admin-user sqladmin \
  --admin-password YourSecurePassword123!

# Create database
az sql db create \
  --resource-group AspNetSpaGroup \
  --server aspnet-spa-db-server \
  --name AspNetSpaTemplate \
  --edition Basic

# Allow Azure services
az sql server firewall-rule create \
  --resource-group AspNetSpaGroup \
  --server aspnet-spa-db-server \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

## AWS Deployment

### Using Elastic Beanstalk

```bash
# Create Elastic Beanstalk environment
eb init -p "Docker running on 64bit Amazon Linux 2" aspnet-spa

# Create environment
eb create aspnet-spa-prod

# Deploy application
eb deploy

# View logs
eb logs

# Monitor environment
eb status
```

### Using EC2

```bash
# Install .NET 10 runtime
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 10.0

# Clone repository
git clone https://github.com/Sarmkadan/aspnet-spa-template.git
cd aspnet-spa-template

# Build and publish
dotnet publish -c Release -o /var/www/aspnet-spa

# Create systemd service
sudo nano /etc/systemd/system/aspnet-spa.service
```

**Service file:**
```ini
[Unit]
Description=ASP.NET SPA Application
After=network.target

[Service]
Type=notify
User=aspnet
WorkingDirectory=/var/www/aspnet-spa
ExecStart=/usr/bin/dotnet /var/www/aspnet-spa/AspNetSpaTemplate.dll
Restart=on-failure
RestartSec=5

[Install]
WantedBy=multi-user.target
```

```bash
# Enable and start service
sudo systemctl enable aspnet-spa
sudo systemctl start aspnet-spa
```

## IIS Deployment

### Publish to IIS

```bash
# Publish for Windows hosting
dotnet publish -c Release -o C:\releases\aspnet-spa-v1.0

# Install hosting bundle
# Download from: https://dotnet.microsoft.com/en-us/download/dotnet

# Create IIS application
# 1. Open IIS Manager
# 2. Right-click "Sites" → "Add Website"
# 3. Configure:
#    - Site name: AspNetSpaTemplate
#    - Physical path: C:\releases\aspnet-spa-v1.0
#    - Port: 443 (HTTPS)
#    - SSL certificate: Your certificate
```

### IIS Configuration

```xml
<!-- web.config -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" 
                arguments=".\AspNetSpaTemplate.dll" 
                stdoutLogEnabled="true" 
                stdoutLogFile=".\logs\stdout" 
                hostingModel="inprocess" />
  </system.webServer>
</configuration>
```

## Linux/VM Deployment

### Ubuntu/Debian

```bash
# Install .NET 10
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 10.0

# Clone and build
git clone https://github.com/Sarmkadan/aspnet-spa-template.git
cd aspnet-spa-template
dotnet publish -c Release

# Install Nginx (reverse proxy)
sudo apt update
sudo apt install nginx

# Configure Nginx
sudo nano /etc/nginx/sites-available/default
```

**Nginx configuration:**
```nginx
upstream aspnet_app {
    server 127.0.0.1:5000;
}

server {
    listen 80;
    listen [::]:80;
    server_name yourdomain.com;

    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://aspnet_app;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

```bash
# Enable SSL with Let's Encrypt
sudo apt install certbot python3-certbot-nginx
sudo certbot certonly --nginx -d yourdomain.com

# Start Nginx
sudo systemctl enable nginx
sudo systemctl start nginx
```

## Post-Deployment Verification

### Health Checks

```bash
# Check application health
curl https://yourdomain.com/api/health

# Verify API endpoints
curl https://yourdomain.com/api/products

# Test authentication
curl -X POST https://yourdomain.com/api/users/login \
  -d '{"email":"user@example.com","password":"password"}'
```

### Database Verification

```bash
# Run migrations
dotnet ef database update

# Verify database
# Connect with SQL Server Management Studio
# Or use sqlcmd:
sqlcmd -S server.database.windows.net -U admin -P password -d AspNetSpaTemplate
```

### Performance Testing

```bash
# Using Apache Bench
ab -n 1000 -c 100 https://yourdomain.com/api/products

# Using Load Impact or JMeter for more complex scenarios
```

## Monitoring and Maintenance

### Application Logging

```csharp
// Configure structured logging
builder.Services.AddLogging(config => {
    config.AddConsole();
    config.AddFile("logs/app-{Date}.txt");
    // Or use Serilog for structured logging
});
```

### Automated Backups

```bash
# Daily SQL Server backup
# Create backup job in SQL Server Agent
BACKUP DATABASE [AspNetSpaTemplate]
TO DISK = 'C:\Backups\AspNetSpaTemplate.bak'
WITH INIT, COMPRESSION
```

### Monitoring Stack

Recommended tools:
- **Application Insights** (Azure) or **New Relic**
- **Log aggregation**: ELK Stack or Datadog
- **Uptime monitoring**: Uptime Robot or StatusPage.io
- **Error tracking**: Sentry or Application Insights

### Update Strategy

```bash
# Rolling update strategy
# 1. Build new image
docker build -t myregistry/aspnet-spa:v2.0 .

# 2. Push to registry
docker push myregistry/aspnet-spa:v2.0

# 3. Update container (zero-downtime if load-balanced)
docker service update --image myregistry/aspnet-spa:v2.0 aspnet-spa

# 4. Verify new version
curl https://yourdomain.com/api/health
```

---

**For questions or issues with deployment, see docs/faq.md or open an issue on GitHub.**
