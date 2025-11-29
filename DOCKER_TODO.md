# 🐳 Docker & Deployment для TimeCafe Microservices

> **Статус:** ✅ Auth и UserProfile готовы к запуску  
> **Обновлено:** 26.01.2025  
> **Архитектура:** Микросервисы на .NET 9

---

## 🎯 Цель

Контейнеризация всех микросервисов TimeCafe для:

- Простого развертывания на production
- Изолированной разработки
- Одинакового окружения у всей команды
- Легкого масштабирования

---

## 📦 Компоненты для Docker

### ✅ Уже работают в Docker:

- ✅ **Redis** - кэширование
- ✅ **PostgreSQL** - основная БД для всех сервисов
- ✅ **RabbitMQ** - message broker для межсервисной коммуникации

### 🔨 Микросервисы для контейнеризации:

#### 1. **Auth Service** (порт 8001)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Services/Auth/Auth.TimeCafe.API/", "Auth.TimeCafe.API/"]
RUN dotnet restore "Auth.TimeCafe.API/Auth.TimeCafe.API.csproj"
RUN dotnet build "Auth.TimeCafe.API/Auth.TimeCafe.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auth.TimeCafe.API/Auth.TimeCafe.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.TimeCafe.API.dll"]
```

#### 2. **UserProfile Service** (порт 8002)

- Аналогичный Dockerfile
- Expose 8002

#### 3. **Visit Service** (порт 8003) - планируется

- После создания сервиса
- Expose 8003

#### 4. **Billing Service** (порт 8004) - планируется

- После интеграции с Robokassa
- Expose 8004

---

## 🔧 Docker Compose

### Структура файлов:

```
TimeCafe/
├── docker-compose.yml              # Production
├── docker-compose.override.yml     # Development overrides
├── docker-compose.test.yml         # Testing environment
└── .env                            # Environment variables
```

### docker-compose.yml (основной):

```yaml
version: "3.8"

services:
  # Infrastructure
  postgres:
    image: postgres:16-alpine
    container_name: timecafe-postgres
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-admin}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-Admin123!}
      POSTGRES_MULTIPLE_DATABASES: AuthDB,ProfileDB,VisitDB,BillingDB
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
      - ./scripts/init-databases.sh:/docker-entrypoint-initdb.d/init-databases.sh
    networks:
      - timecafe-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U admin"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: timecafe-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - timecafe-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  rabbitmq:
    image: rabbitmq:3.12-management-alpine
    container_name: timecafe-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER:-admin}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD:-Admin123!}
    ports:
      - "5672:5672" # AMQP
      - "15672:15672" # Management UI
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    networks:
      - timecafe-network
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 30s
      timeout: 10s
      retries: 5

  # Microservices
  auth-service:
    build:
      context: .
      dockerfile: Services/Auth/Auth.TimeCafe.API/Dockerfile
    container_name: timecafe-auth
    environment:
      ASPNETCORE_ENVIRONMENT: ${ENVIRONMENT:-Production}
      ASPNETCORE_URLS: http://+:8001
      ConnectionStrings__DefaultConnection: Host=postgres;Database=AuthDB;Username=${POSTGRES_USER:-admin};Password=${POSTGRES_PASSWORD:-Admin123!}
      ConnectionStrings__Redis: redis:6379
      RabbitMQ__Host: rabbitmq
      RabbitMQ__Username: ${RABBITMQ_USER:-admin}
      RabbitMQ__Password: ${RABBITMQ_PASSWORD:-Admin123!}
    ports:
      - "8001:8001"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    networks:
      - timecafe-network
    restart: unless-stopped

  profile-service:
    build:
      context: .
      dockerfile: Services/UserProfile/UserProfile.TimeCafe/UserProfile.TimeCafe.API/Dockerfile
    container_name: timecafe-profile
    environment:
      ASPNETCORE_ENVIRONMENT: ${ENVIRONMENT:-Production}
      ASPNETCORE_URLS: http://+:8002
      ConnectionStrings__DefaultConnection: Host=postgres;Database=ProfileDB;Username=${POSTGRES_USER:-admin};Password=${POSTGRES_PASSWORD:-Admin123!}
      ConnectionStrings__Redis: redis:6379
      RabbitMQ__Host: rabbitmq
      RabbitMQ__Username: ${RABBITMQ_USER:-admin}
      RabbitMQ__Password: ${RABBITMQ_PASSWORD:-Admin123!}
    ports:
      - "8002:8002"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    networks:
      - timecafe-network
    restart: unless-stopped

  # visit-service:  # Будет добавлен позже
  # billing-service:  # Будет добавлен позже

  # Frontend (опционально в Docker)
  webapp:
    build:
      context: ./WebApp/timecafe.react.ui
      dockerfile: Dockerfile
    container_name: timecafe-webapp
    environment:
      VITE_AUTH_API_URL: http://localhost:8001
      VITE_PROFILE_API_URL: http://localhost:8002
      VITE_VISIT_API_URL: http://localhost:8003
      VITE_BILLING_API_URL: http://localhost:8004
    ports:
      - "3000:80"
    depends_on:
      - auth-service
      - profile-service
    networks:
      - timecafe-network
    restart: unless-stopped

networks:
  timecafe-network:
    driver: bridge

volumes:
  postgres-data:
  redis-data:
  rabbitmq-data:
```

### .env файл:

```env
# Database
POSTGRES_USER=admin
POSTGRES_PASSWORD=Admin123!

# RabbitMQ
RABBITMQ_USER=admin
RABBITMQ_PASSWORD=Admin123!

# Environment
ENVIRONMENT=Development

# JWT Settings
JWT_SECRET=your-super-secret-key-change-in-production
JWT_ISSUER=TimeCafe
JWT_AUDIENCE=TimeCafeAPI

# External Services
TWILIO_ACCOUNT_SID=your_account_sid
TWILIO_AUTH_TOKEN=your_auth_token
TWILIO_PHONE_NUMBER=+1234567890

POSTMARK_API_KEY=your_postmark_key

ROBOKASSA_MERCHANT_LOGIN=your_login
ROBOKASSA_PASSWORD_1=your_password_1
ROBOKASSA_PASSWORD_2=your_password_2
```

### scripts/init-databases.sh:

```bash
#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" <<-EOSQL
    CREATE DATABASE "AuthDB";
    CREATE DATABASE "ProfileDB";
    CREATE DATABASE "VisitDB";
    CREATE DATABASE "BillingDB";
EOSQL
```

---

## 🚀 Команды для работы

### ⚙️ Подготовка перед первым запуском:

```powershell
# 1. Скопировать .env.example в .env и заполнить реальные значения
Copy-Item .env.example .env

# 2. Отредактировать .env файл:
# - JWT_SECRET (минимум 32 символа)
# - S3_SECRET_KEY (ваш секретный ключ Selectel)
# - GOOGLE_CLIENT_ID/SECRET, VK_CLIENT_ID/SECRET (если используете OAuth)
# - TWILIO_ACCOUNT_SID/AUTH_TOKEN (если используете SMS)
# - POSTMARK_SERVER_TOKEN (если используете email)
notepad .env
```

### 🚀 Первый запуск:

```powershell
# 1. Собрать все образы
docker-compose build

# 2. Запустить все контейнеры
docker-compose up -d

# 3. Проверить, что все сервисы запустились
docker-compose ps

# 4. Дождаться инициализации PostgreSQL (проверить логи)
docker-compose logs postgres

# 5. Применить миграции EF Core
docker-compose exec auth-service dotnet ef database update --project Auth.TimeCafe.Infrastructure --startup-project Auth.TimeCafe.API
docker-compose exec profile-service dotnet ef database update --project UserProfile.TimeCafe.Infrastructure --startup-project UserProfile.TimeCafe.API

# 6. Проверить логи сервисов
docker-compose logs -f auth-service
docker-compose logs -f profile-service

# 7. Проверить health endpoints
curl http://localhost:8001/health
curl http://localhost:8002/health

# 8. Открыть Scalar API документацию
# Auth API: http://localhost:8001/scalar/v1
# Profile API: http://localhost:8002/scalar/v1
```

### Разработка:

```powershell
# Запустить только инфраструктуру (БД, Redis, RabbitMQ)
docker-compose up -d postgres redis rabbitmq

# Запустить сервисы локально (в VS Code / Visual Studio)
# Auth Service: dotnet run --project Services/Auth/Auth.TimeCafe.API
# Profile Service: dotnet run --project Services/UserProfile/.../UserProfile.TimeCafe.API
```

### Остановка:

```powershell
# Остановить все контейнеры
docker-compose down

# Остановить и удалить volumes (⚠️ удалит данные!)
docker-compose down -v
```

### Просмотр состояния:

```powershell
# Список контейнеров
docker-compose ps

# Логи конкретного сервиса
docker-compose logs auth-service

# Статистика использования ресурсов
docker stats
```

### Обновление:

```powershell
# Пересобрать конкретный сервис
docker-compose build auth-service

# Перезапустить конкретный сервис
docker-compose restart auth-service

# Обновить и перезапустить
docker-compose up -d --build auth-service
```

---

## 🏗️ Архитектура в Docker

```
┌─────────────────────────────────────────────────────────┐
│                    Docker Host (localhost)              │
│                                                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │           timecafe-network (bridge)              │  │
│  │                                                  │  │
│  │  ┌──────────────┐  ┌──────────────┐             │  │
│  │  │ auth-service │  │profile-service│            │  │
│  │  │   :8001      │  │   :8002      │             │  │
│  │  └──────┬───────┘  └──────┬───────┘             │  │
│  │         │                  │                     │  │
│  │  ┌──────┴──────────────────┴─────────┐          │  │
│  │  │                                    │          │  │
│  │  │  ┌──────────┐  ┌───────┐  ┌────┐ │          │  │
│  │  │  │PostgreSQL│  │RabbitMQ│  │Redis│          │  │
│  │  │  │  :5432   │  │ :5672 │  │:6379│ │          │  │
│  │  │  └──────────┘  │:15672 │  └────┘ │          │  │
│  │  │                └───────┘          │          │  │
│  │  └────────────────────────────────────┘          │  │
│  │                                                  │  │
│  │  ┌──────────────┐                               │  │
│  │  │   webapp     │                               │  │
│  │  │   :3000      │                               │  │
│  │  └──────────────┘                               │  │
│  └──────────────────────────────────────────────────┘  │
│                                                         │
│  Exposed Ports:                                        │
│  - 5432  (PostgreSQL)                                  │
│  - 6379  (Redis)                                       │
│  - 5672  (RabbitMQ AMQP)                              │
│  - 15672 (RabbitMQ Management)                        │
│  - 8001  (Auth API)                                   │
│  - 8002  (Profile API)                                │
│  - 3000  (WebApp)                                     │
└─────────────────────────────────────────────────────────┘
```

---

## 📋 TODO List

### ✅ Выполнено:

- [x] Настроен Redis в docker-compose
- [x] Настроен PostgreSQL в docker-compose
- [x] Настроен RabbitMQ в docker-compose
- [x] **Auth Service Dockerfile** создан с multi-stage build, health check
- [x] **UserProfile Service Dockerfile** создан с multi-stage build, health check
- [x] **docker-compose.yml** создан с полной конфигурацией всех сервисов
- [x] **.env.example** создан с шаблоном всех переменных окружения
- [x] **scripts/init-databases.sh** создан для инициализации AuthDB и ProfileDB
- [x] **Health checks** настроены для всех сервисов (postgres, redis, rabbitmq, auth, profile)
- [x] **Environment variables** настроены для Auth (JWT, OAuth, Twilio, Postmark)
- [x] **Environment variables** настроены для UserProfile (S3, Sightengine)

### 🔄 В процессе:

- [ ] Применить миграции EF Core при первом запуске (см. инструкцию ниже)

### 📅 Запланировано:

- [ ] Visit Service Dockerfile (после создания сервиса)
- [ ] Billing Service Dockerfile (после создания сервиса)
- [ ] WebApp Dockerfile (React)
- [ ] DesktopApp учитывает Docker endpoints
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Production deployment (Azure/AWS/VPS)
- [ ] Monitoring (Prometheus + Grafana)
- [ ] Logging (ELK Stack)

---

## 🔒 Security Notes

### Development:

- Используем простые пароли в .env для разработки
- Все порты открыты для localhost

### Production:

- ⚠️ **Обязательно** изменить все пароли
- ⚠️ Использовать Docker Secrets для чувствительных данных
- ⚠️ Закрыть ненужные порты (оставить только API Gateway)
- ⚠️ Настроить HTTPS (Let's Encrypt)
- ⚠️ Использовать environment-specific .env файлы

```yaml
# docker-compose.prod.yml (пример)
services:
  auth-service:
    secrets:
      - db_password
      - jwt_secret
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=AuthDB;Username=admin;Password=/run/secrets/db_password"
      JWT__Secret: "/run/secrets/jwt_secret"

secrets:
  db_password:
    external: true
  jwt_secret:
    external: true
```

---

## 📊 Мониторинг

### Health Checks:

```powershell
# Проверка здоровья сервисов
curl http://localhost:8001/health
curl http://localhost:8002/health

# RabbitMQ Management UI
# http://localhost:15672 (admin/Admin123!)
```

### Logs:

```powershell
# Real-time логи всех сервисов
docker-compose logs -f

# Логи только Auth Service
docker-compose logs -f auth-service

# Последние 100 строк
docker-compose logs --tail=100 profile-service
```

---

## 🐛 Troubleshooting

### Проблема: Контейнер не стартует

```powershell
# Проверить логи
docker-compose logs service-name

# Проверить health check
docker inspect timecafe-auth | grep Health -A 10

# Перезапустить
docker-compose restart service-name
```

### Проблема: База данных не доступна

```powershell
# Проверить PostgreSQL
docker-compose exec postgres psql -U admin -d AuthDB

# Проверить подключение
docker-compose exec auth-service dotnet ef database update
```

### Проблема: RabbitMQ не работает

```powershell
# Проверить статус
docker-compose exec rabbitmq rabbitmqctl status

# Проверить очереди
docker-compose exec rabbitmq rabbitmqctl list_queues
```

---

---

## 🎉 Готово к запуску!

### Архитектура контейнеризации:

- **Auth Service** (`:8001`) - аутентификация, JWT, OAuth, SMS, Email
- **UserProfile Service** (`:8002`) - профили, фото с модерацией, S3 storage
- **PostgreSQL** (`:5432`) - базы данных AuthDB и ProfileDB
- **Redis** (`:6379`) - кэширование
- **RabbitMQ** (`:5672`, `:15672`) - межсервисная коммуникация

### Переменные окружения:

Все конфигурации вынесены в `.env` файл:

- ✅ Секреты не хранятся в коде
- ✅ Легко переключаться между Development/Production
- ✅ Каждый сервис получает только нужные ему переменные

### Health Checks:

Все сервисы имеют health check endpoints:

- Инфраструктура: postgres, redis, rabbitmq
- Микросервисы: auth-service, profile-service
- `docker-compose ps` покажет состояние здоровья каждого сервиса

---

Обновлено: 26.01.2025  
Статус: ✅ **Auth и UserProfile готовы к production!**
