# TimeCafe

![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![React 18](https://img.shields.io/badge/React-18-61DAFB?logo=react&logoColor=000)
![PostgreSQL 16](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-4.2.2-FF6600?logo=rabbitmq&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-8.4-DC382D?logo=redis&logoColor=white)
![Elasticsearch](https://img.shields.io/badge/Elasticsearch-9.2-005571?logo=elasticsearch&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)

Платформа для управления тайм-кафе на микросервисной архитектуре: регистрация и безопасность, профили пользователей, визиты, биллинг и событийное взаимодействие.

## Архитектура

- Микросервисы с отдельной базой данных на сервис
- CQRS + MediatR
- Event-driven коммуникация (MassTransit + RabbitMQ)
- Carter endpoints
- Чистая архитектура: API → Application → Domain → Infrastructure
- API Gateway на YARP (YarpProxy)
- Логирование Serilog с отправкой в Elasticsearch и просмотром в Kibana

## Сервисы

| Сервис | Порт | Назначение |
| --- | --- | --- |
| Auth | 8001 | Аутентификация, подтверждение email/SMS, смена пароля |
| UserProfile | 8002 | Профили пользователей, фото, кэширование профилей |
| Venue | 8003 | Тарифы, визиты, публикация событий |
| Billing | 8004 | Баланс и транзакции |
| YarpProxy | - | API Gateway и маршрутизация запросов |

## Фронтенд и клиенты

- WebApp: WebApp/timecafe.react.ui (React 18 + Vite)
- DesktopApp: DesktopApp/TimeCafe.UI

## Стек

- Backend: .NET 9, EF Core, PostgreSQL 16, MediatR, Carter, MassTransit
- Frontend: React 18, Vite, Redux Toolkit, Fluent UI, Tailwind (layout/spacing)
- Инфраструктура: Docker Compose, Redis, RabbitMQ
- API Gateway: YARP (YarpProxy)
- Логи и мониторинг: Serilog, Elasticsearch, Kibana
- Хранилище медиа: S3-совместимое (фото профиля)
- Внешние сервисы: Twilio (SMS), Postmark (Email), Google/Microsoft OAuth, reCAPTCHA, Sightengine

## Инфраструктура (Docker Compose)

Сервисы инфраструктуры поднимаются через docker-compose:

- PostgreSQL 16
- Redis
- RabbitMQ
- Elasticsearch + Kibana

## Быстрый старт

1. Запуск инфраструктуры:

```bash
docker-compose up -d postgres redis rabbitmq elasticsearch kibana
```

1. Запуск сервиса (пример для UserProfile):

```bash
cd Services/UserProfile/UserProfile.TimeCafe.API
dotnet run
```

1. Запуск фронтенда:

```bash
cd WebApp/timecafe.react.ui
npm install
npm run dev -- --host
```

## Конфигурация

Настройки находятся в appsettings.json / appsettings.Development.json сервисов и в appsettings.shared.json.
Ключевые параметры:

- JWT (issuer, audience, signing key)
- Postmark (email)
- Twilio (SMS)
- S3 (access key, secret key, service URL, bucket)
- RabbitMQ, Redis, PostgreSQL
- Serilog → Elasticsearch

## Интеграции

- Twilio — SMS подтверждения
- Postmark — email доставка
- S3 — хранение фото профиля
- Sightengine — проверка контента изображений

## Тесты

```bash
dotnet test --settings .runsettings
```
