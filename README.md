# TimeCafe ☕

<div align="center">
 <img src="https://img.shields.io/badge/TimeCafe-%F0%9F%8D%B5%20%F0%9F%8C%B1%20%F0%9F%91%A8%F0%9F%8F%BB%F0%9F%8F%92-blue" alt="TimeCafe" />
	<h2>Платформа для управления тайм-кафе</h2>
	<p>
		<img src="https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet" alt=".NET 9" />
		<img src="https://img.shields.io/badge/React-18-61DAFB?logo=react" alt="React" />
		<img src="https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql" alt="PostgreSQL" />
		<img src="https://img.shields.io/badge/RabbitMQ-3.12-FF6600?logo=rabbitmq" alt="RabbitMQ" />
		<img src="https://img.shields.io/badge/Docker-ready-2496ED?logo=docker" alt="Docker" />
	</p>
</div>

---

## 🚀 О проекте

**TimeCafe** — современная микросервисная платформа для управления тайм-кафе с:

- 🌐 **Веб-интерфейсом** (React + TypeScript + Vite)
- 🔌 **Микросервисной архитектурой** (.NET 9, Clean Architecture, CQRS)
- 🐳 **Контейнеризацией** (Docker, docker-compose)
- 📨 **Event-driven коммуникацией** (RabbitMQ + MassTransit)
- 🗄️ **Database per Service** (PostgreSQL)

> **⚠️ Main Service (Legacy) в процессе полной миграции на микросервисы**

## 🏗️ Архитектура

Проект построен на основе **микросервисной архитектуры** с использованием паттерна **Database per Service**:

```text
TimeCafe/
├── Services/                        # Микросервисы
│   ├── Auth/                        # ✅ Аутентификация и авторизация
│   │   ├── Auth.TimeCafe.API/
│   │   ├── Auth.TimeCafe.Application/
│   │   ├── Auth.TimeCafe.Domain/
│   │   ├── Auth.TimeCafe.Infrastructure/
│   │   └── Auth.TimeCafe.Test/
│   │
│   ├── UserProfile/                 # ✅ Профили пользователей
│   │   └── UserProfile.TimeCafe/
│   │       ├── UserProfile.TimeCafe.API/
│   │       ├── UserProfile.TimeCafe.Application/
│   │       ├── UserProfile.TimeCafe.Domain/
│   │       ├── UserProfile.TimeCafe.Infrastructure/
│   │       └── UserProfile.TimeCafe.Test/
│   │
│   ├── Main/                        # ⚠️ LEGACY - будет удалён
│   │   └── Main.TimeCafe.*/         # Используется только для понимания старой логики
│   │
│   ├── Finance/ (в разработке)      # 🔜 Финансы, платежи, баланс
│   └── Venue/ (в разработке)        # 🔜 Заведение, визиты, тарифы
│
├── DesktopApp/                      # 🖥️ WinUI3 приложение (отложено)
│   └── TimeCafe.UI/
│
├── WebApp/                          # 🌐 React веб-приложение
│   └── timecafe.react.ui/
│
├── BuildingBlocks/                  # 📦 Общие компоненты
│   ├── Behaviors/                   # MediatR behaviors
│   ├── Extensions/                  # CQRS результаты
│   └── Middleware/                  # Exception handling
│
└── diagrams/                        # 📊 UML диаграммы
    ├── flowchart/
    └── UML/
```

### 🔗 Межсервисная коммуникация

```
┌─────────────┐  UserRegistered  ┌──────────────┐
│ Auth Service│─────────────────→│UserProfile Svc│
└─────────────┘                   └──────────────┘

┌─────────────┐ VisitCompleted   ┌──────────────┐
│Venue Service│─────────────────→│Finance Service│
└─────────────┘ (UserId, Amount)  └──────────────┘
```

---

## 🎯 Микросервисы

### 🔐 Auth Service ✅

**Назначение:** Аутентификация, авторизация, управление токенами

**Функциональность:**

- 🔑 JWT аутентификация (Access + Refresh токены)
- 📧 Email подтверждение (Postmark)
- 📱 SMS верификация (Twilio)
- 🔒 Permission-based авторизация
- 🔄 Refresh Token Rotation
- 👥 ASP.NET Core Identity

**Технологии:**

- ASP.NET Core 9.0, Identity
- EF Core, PostgreSQL
- MediatR, FluentValidation
- JWT Bearer Authentication
- Twilio (SMS), Postmark (Email)

**API Endpoints:**

```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/change-password
POST /api/auth/verify-phone
```

---

### 👤 UserProfile Service ✅

**Назначение:** Управление профилями пользователей

**Функциональность:**

- 📝 CRUD операции с профилями
- 📄 Постраничная выборка
- 💾 Redis кэширование
- 📝 Дополнительные заметки о пользователе (новое!)
- 🚫 Причины блокировки

**Модели:**

```csharp
Profile:
  - UserId (PK)
  - FirstName, LastName, MiddleName
  - BirthDate, Gender
  - AccessCardNumber, PhotoUrl
  - ProfileStatus, BanReason

AdditionalInfo:
  - InfoId (PK)
  - UserId (FK)
  - InfoText
  - CreatedAt, CreatedBy
```

**API Endpoints:**

```
GET    /api/profiles
GET    /api/profiles/{userId}
POST   /api/profiles
PUT    /api/profiles/{userId}
DELETE /api/profiles/{userId}

POST   /api/profiles/{userId}/notes
GET    /api/profiles/{userId}/notes
```

---

### 💰 Finance Service 🔜

**Назначение:** Финансовые операции, платежи, баланс

**Планируемая функциональность:**

- 💵 Управление балансом клиентов
- 📝 История транзакций
- 💳 Пополнение депозита
- 💸 Списание при завершении визита
- 🔗 Интеграция с кассой (Robokassa)
- 📊 Расчёт задолженностей
- 📨 Слушает `VisitCompletedEvent` → списывает деньги

**Модели:**

```csharp
Balance:
  - UserId (PK)
  - CurrentBalance, Debt
  - TotalDeposited, TotalSpent

Transaction:
  - TransactionId (PK)
  - UserId (FK)
  - Amount, Type (Deposit/Withdrawal)
  - Source (Visit/Manual/Payment)

Payment:
  - PaymentId (PK)
  - Amount, Status
  - ExternalPaymentId (от кассы)
```

---

### 🏢 Venue Service 🔜

**Назначение:** Управление заведением, визитами, тарифами

**Планируемая функциональность:**

- 🏢 Управление тарифами (почасовая/поминутная)
- ⏰ Режим работы (12:00 - 02:00)
- 👥 Визиты клиентов (вход/выход)
- ⏱️ Расчёт времени и стоимости
- 🎨 Темы оформления
- 🎁 Акции и промо
- 📨 Публикует `VisitCompletedEvent` → Finance списывает

**Модели:**

```csharp
Tariff:
  - TariffId (PK)
  - Name (Льготный, Зимний)
  - HourlyRate, MinuteRate
  - ThemeId (FK)

Visit:
  - VisitId (PK)
  - UserId, TariffId
  - BillingTypeId (Почасовая/Поминутная)
  - EntryTime, ExitTime
  - CalculatedCost, Status

Promotion:
  - PromotionId (PK)
  - Name, Description
  - DiscountPercent
  - ValidFrom, ValidTo
```

---

## 🧩 Технологический стек

### Backend:

- **.NET 9.0** - основной фреймворк
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - реляционная БД (Npgsql)
- **Redis** - кэш и сессии
- **RabbitMQ** - message broker
- **MassTransit** - event-driven коммуникация
- **MediatR** - CQRS паттерн
- **FluentValidation** - валидация
- **Swagger/OpenAPI** - документация API
- **Twilio** - SMS сервис
- **Postmark** - Email сервис

### Frontend:

- **React 18** - UI библиотека
- **TypeScript** - типизация
- **Vite** - сборщик
- **TailwindCSS** - стилизация
- **React Router** - роутинг

### Desktop (отложено):

- **WinUI3** - десктопное приложение
- **MVVM** - архитектурный паттерн
- **.NET 9** - фреймворк

### DevOps:

- **Docker** - контейнеризация
- **docker-compose** - оркестрация
- **GitHub Actions** - CI/CD
- **PlantUML** - UML диаграммы

---

## 📚 Документация

- 🎯 [REAL_MIGRATION_PLAN.md](./REAL_MIGRATION_PLAN.md) - **РЕАЛЬНЫЙ** план миграции
- 🚀 [QUICK_START.md](./QUICK_START.md) - краткое резюме для быстрого старта
- 🐳 [DOCKER_TODO.md](./DOCKER_TODO.md) - Docker setup
- 🐳 [DOCKER_TODO.md](./DOCKER_TODO.md) - Docker setup
- 📊 [MIGRATION_SUMMARY.md](./MIGRATION_SUMMARY.md) - краткое резюме
- 🔐 [Identity-Guide.md](./Services/Auth/Identity-Guide.md) - Auth Service

### UML Диаграммы:

- `diagrams/UML/diagramClass-auth.md` - class diagram
- `diagrams/UML/sequence-auth.md` - sequence diagram
- `diagrams/UML/usecase-auth.puml` - use case diagram

---

## 👨‍💻 Авторы и контакты

- OnlyUp2219 (архитектор, разработчик)
- [GitHub репозиторий](https://github.com/OnlyUp2219/TimeCafeWinUI3)

---

_Проект находится в активной разработке. Добро пожаловать к сотрудничеству!_

## Быстрый старт

### Предварительные требования

- .NET 9.0 SDK
- Node.js & npm
- JDK (для генерации UML)
- Graphviz (для визуализации диаграмм)

### Сборка и запуск

#### Десктопное приложение

```powershell
cd DesktopApp/TimeCafe.UI
# Сборка
dotnet build
# Запуск
start TimeCafe.UI.exe
```

#### Веб-клиент

```powershell
cd WebApp/timecafe.react.ui
npm install
npm run dev
```

#### Микросервисы

```powershell
cd Services/Main/Main.TimeCafe.API
# Сборка и запуск
dotnet run
```

## Используемые технологии

- **Backend**: ASP.NET Core, C#, Entity Framework
- **Frontend**: React, Vite, TailwindCSS
- **Desktop**: WinUI3
- **DevOps**: Docker, CI/CD, Graphviz, PlantUML

## Авторы и контакты

- OnlyUp2219 (архитектор, разработчик)
- [GitHub репозиторий](https://github.com/OnlyUp2219/TimeCafeWinUI3)

---

_Проект находится в активной разработке. Добро пожаловать к сотрудничеству!_
