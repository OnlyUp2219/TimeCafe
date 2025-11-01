# Правила кодирования для TimeCafe

## Стиль коммитов
Формат: `<тип>: <описание>`

Типы:
- `features` - новая функциональность
- `refactor` - рефакторинг кода
- `fix` - исправление багов

Примеры:
```
refactor: реализовать унифицированную обработку ошибок, создать ErrorDetail и ErrorType классы
features: добавить страницу подтверждения email
fix: исправить капчу в модальном окне
```

## Код
- **НЕ писать комментарии в коде** (код должен быть самодокументируемым)
- Использовать чистую архитектуру (Clean Architecture)
- CQRS паттерн для команд и запросов
- Result Pattern для обработки ошибок
- FluentValidation для валидации

## Структура
- Backend: .NET 9, ASP.NET Core, Entity Framework Core, Identity
- Frontend: React + TypeScript + Vite
- BuildingBlocks: общие компоненты для переиспользования

## Именование
- Классы: PascalCase
- Методы: PascalCase
- Переменные: camelCase
- Константы: PascalCase
- Интерфейсы: IInterface

## TypeScript/React
- Использовать functional components с hooks
- TypeScript strict mode
- Избегать `any`, использовать конкретные типы
- Использовать `interface` для объектов, `type` для union/intersection
