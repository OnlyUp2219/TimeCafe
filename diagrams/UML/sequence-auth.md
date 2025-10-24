# Сценарии аутентификации (Mermaid)

Файл разбит на отдельные диаграммы вместо большого блока с `par`. Оставлены только фактически реализованные эндпоинты из `Auth.TimeCafe.API`:

- `POST /registerWithUsername`
- `POST /login-jwt`
- `POST /refresh-token-jwt`
- `POST /forgot-password-link`
- Внешние провайдеры: `GET /authenticate/login/{google|microsoft}` и их `.../callback`

Не включены: вымышленные `/auth/logout`, `/admin/users/block`, произвольные `/data/*` — таких маршрутов нет в текущем коде. Роли выдаются внутренне через `AddToRoleAsync`, отдельного сервиса ролей нет, поэтому участник `RoleSvc` удалён. Email отправка в reset пароле закомментирована — показываем её как опциональную.

## Регистрация пользователя

```mermaid
sequenceDiagram
autonumber
participant UI as UI (React)
participant Auth as Auth API
participant Bus as EventBus
UI->>Auth: POST /registerWithUsername
Auth-->>UI: 200 Tokens(Access,Refresh)
Auth-->>Bus: Publish UserRegisteredEvent
note over Auth: Создание IdentityUser
note over Bus: Fanout событие
```

## Логин и получение токенов

```mermaid
sequenceDiagram
autonumber
participant UI as UI
participant Auth as Auth API
UI->>Auth: POST /login-jwt
Auth-->>UI: 200 Tokens(Access,Refresh)
note over Auth: Проверка email + пароля
```

## Обновление токена (Refresh Flow)

```mermaid
sequenceDiagram
autonumber
participant UI as UI
participant Auth as Auth API
UI->>Auth: POST /refresh-token-jwt
Auth-->>UI: 200 New Tokens
note over Auth: Валидация refresh токена
```

## Сброс пароля (Forgot Password)

```mermaid
sequenceDiagram
autonumber
participant UI as UI
participant Auth as Auth API
participant (Mail) as Email? optional
UI->>Auth: POST /forgot-password-link
Auth-->>UI: 200 {callbackUrl}
Auth-->>(Mail): (опционально) Send reset link
note over Auth: Генерация reset token + Base64Url
```

## Logout (Выход пользователя)

```mermaid
sequenceDiagram
autonumber
participant UI as UI
participant Auth as Auth API
UI->>Auth: POST /logout {refreshToken}
Alt токен найден и активен
	Auth->>Auth: IsRevoked = true
	Auth-->>UI: 200 { message: "Logged out", revoked: true }
Else токен отсутствует / уже ревокнут
	Auth-->>UI: 200 { message: "Logged out", revoked: false }
End
note over UI: Клиент очищает Access/Refresh локально
```

## Внешняя авторизация (Google пример)

```mermaid
sequenceDiagram
autonumber
participant UI as UI
participant Google as Google OAuth
participant Auth as Auth API
UI->>Google: GET /authenticate/login/google
Google-->>UI: 302 /authenticate/login/google/callback?returnUrl=...
UI->>Auth: GET /authenticate/login/google/callback
Auth->>Google: Validate external principal
Google-->>Auth: Email, ExternalId
Auth->>Auth: FindOrCreate IdentityUser
Auth->>Auth: AddToRole(client)
Auth-->>UI: 302 returnUrl#access_token=...&refresh_token=...
```

## Внешняя авторизация (Microsoft пример)

```mermaid
sequenceDiagram
autonumber
participant UI as UI
participant MS as Microsoft OAuth
participant Auth as Auth API
UI->>MS: GET /authenticate/login/microsoft
MS-->>UI: 302 /authenticate/login/microsoft/callback?returnUrl=...
UI->>Auth: GET /authenticate/login/microsoft/callback
Auth->>MS: Validate external principal
MS-->>Auth: Email, ExternalId
Auth->>Auth: FindOrCreate IdentityUser
Auth->>Auth: AddToRole(client)
Auth-->>UI: 302 returnUrl#access_token=...&refresh_token=...
```

## Защищённый ресурс (пример теста)

```mermaid
sequenceDiagram
autonumber
participant UI as UI
participant Auth as Auth API
UI->>Auth: GET /protected-test (Bearer AccessToken)
Alt токен валиден
	Auth-->>UI: 200 "Protected OK"
Else токен невалиден / отсутствует
	Auth-->>UI: 401 Unauthorized
End
```

> Если появятся новые эндпоинты (logout, блокировка пользователей и т.п.), их можно оформить дополнительными диаграммами ниже.

### Возможные дальнейшие улучшения

- Добавить диаграмму для сценария публикации событий (детализация брокера).
- Отдельно показать поток смены пароля (если добавится реальная отправка Email).
- Добавить секцию ошибок: неверный пароль, просроченный refresh.

При необходимости могу создать PlantUML версии для каждого блока в отдельном файле (`sequence-google.puml`, и т.д.). Напишите, если нужно.
