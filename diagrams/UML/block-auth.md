# Auth Module — Блок-схемы (Mermaid)

Ниже представлены основные блок-схемы (flow diagrams) для ключевых процессов модуля авторизации. Они дополняют `sequence-auth.md`, фокусируясь на логике принятия решений и шагах обработки.

## 1. Регистрация пользователя (`POST /registerWithUsername`)

```mermaid
flowchart TD
    A[UI Регистрация] --> B[POST /register]
    B --> C{Есть пользователь?}
    C -->|Да| E[Ошибка 409 Conflict]
    C -->|Нет| D[Создать учет]
    D --> F[Сохранить БД]
    F --> G[Событие UserReg]
    G --> H[Выдать токены]
    H --> I[200 Tokens]

    classDef boxRect font-size:8px,font-family:Arial;
    class A,B,C,E,D,F,G,H,I boxRect;
```

### Ключевые моменты

- Проверка уникальности (email/username)
- Создание через ASP.NET Identity
- Публикация события для других сервисов
- Выдача пары токенов (access + refresh)

> Ошибка при повторной регистрации: всегда 409 Conflict (дубликат).

## 2. Логин и обновление токенов (`POST /login-jwt`, `POST /refresh-token-jwt`)

```mermaid
flowchart TD
    A[UI Логин] --> B[POST /login]
    B --> C{Есть пользователь?}
    C -->|Нет| Z[401]
    C -->|Да| D{Пароль верен?}
    D -->|Нет| Z
    D -->|Да| E[Токены выдать]
    E --> F[200 Tokens]
    F --> G[Store refresh]
    G --> H[Access истек]
    H --> J[POST /refresh]
    J --> K{Refresh валиден?}
    K -->|Нет| X[401 Login]
    K -->|Да| L[Ротация]
    L --> M[200 New]

    classDef boxRect font-size:8px,font-family:Arial;
    class A,B,C,Z,D,E,F,G,H,J,K,X,L,M boxRect;
```

### Ключевые проверки

- Существование пользователя
- Валидация пароля
- Проверка refresh токена (просрочен / отозван)
- Ротация refresh токена при успешном обновлении
  > Ограничение: Mermaid не умеет фиксировать одинаковую ширину прямоугольников. Добиться визуального равенства можно только подбором длины текста и шрифта. Для строгой одинаковой ширины используйте PlantUML.

Альтернатива (PlantUML для логина):

```plantuml
@startuml
skinparam DefaultFontName Arial
skinparam RectangleFontSize 8
rectangle "UI Логин" as A
rectangle "POST /login" as B
rectangle "Есть пользователь?" as C
A --> B --> C
rectangle "Пароль верен?" as D
C --> D
rectangle "401" as Z
C --> Z
D --> Z
rectangle "Токены выдать" as E
D --> E
rectangle "200 Tokens" as F
E --> F
@enduml
```

## 3. Внешняя OAuth авторизация (Google / Microsoft)

```mermaid
flowchart TD
    A[UI: внешняя авторизация] --> B[GET /authenticate/login/provider]
    B --> C[Redirect на OAuth провайдера]
    C --> D[Callback /authenticate/login/provider/callback]
    D --> E[Извлечь claims]
    E --> F{Пользователь существует?}
    F -->|Нет| G[Создать + назначить роль]
    F -->|Да| H[Загрузить профиль]
    G --> I[Пользователь готов]
    H --> I
    I --> J[Выдать токены]
    J --> K[Redirect returnUrl#tokens]
```

### Особенности

- Использование внешнего claim (email, provider key)
- Идемпотентное создание пользователя (FindOrCreate)
- Возврат через hash/фрагмент для SPA
