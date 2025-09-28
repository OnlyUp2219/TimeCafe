# Identity Features TODO List

## 1. Custom User Fields (Дополнительные поля пользователя)

### Что нужно сделать:

- [ ] Создать `ApplicationUser.cs` с дополнительными полями:
  - FirstName, LastName, MiddleName
  - DateOfBirth, Address, City, Country
  - IsPhoneNumberConfirmed, CreatedAt, LastLoginAt
- [ ] Обновить `ApplicationDbContext.cs` для работы с ApplicationUser
- [ ] Обновить `Program.cs` - заменить IdentityUser на ApplicationUser
- [ ] Создать миграцию базы данных
- [ ] Создать страницы редактирования профиля в Areas/Identity/Pages/Account/Manage/

## 2. Roles (Роли пользователей)

### Что нужно сделать:

- [ ] Добавить поддержку ролей в Program.cs: `.AddRoles<IdentityRole>()`
- [ ] Создать роли: Admin, User, Moderator
- [ ] Создать страницы управления ролями
- [ ] Добавить авторизацию по ролям в контроллеры

## 3. Phone Number Verification (Подтверждение телефона)

### Что нужно сделать:

- [ ] Установить Twilio пакет: `Install-Package Twilio`
- [ ] Создать `SmsService.cs` для отправки SMS
- [ ] Добавить конфигурацию Twilio в appsettings.json
- [ ] Создать страницы для добавления/подтверждения телефона
- [ ] Добавить валидацию номера телефона

## 4. Two-Factor Authentication (2FA)

### Что нужно сделать:

- [ ] Добавить поддержку 2FA в Program.cs: `.AddDefaultTokenProviders()`
- [ ] Настроить 2FA опции в IdentityOptions
- [ ] Создать страницы для настройки 2FA:
  - SMS через Twilio
  - TOTP (Google Authenticator, Authy)
  - Email codes
  - Backup codes
- [ ] Добавить логику проверки 2FA при входе

## 5. Additional Features (Дополнительные функции)

### Что можно добавить:

- [ ] Account lockout после неудачных попыток входа
- [ ] Password requirements (сложность пароля)
- [ ] Session management
- [ ] Audit logging (логирование действий)
- [ ] Email change с подтверждением
- [ ] Account deletion
- [ ] External logins (Facebook, Twitter, GitHub)

## 6. UI Pages (Страницы интерфейса)

### Страницы в Areas/Identity/Pages/Account/Manage/:

- [ ] Index.cshtml - редактирование профиля
- [ ] Email.cshtml - смена email
- [ ] ChangePassword.cshtml - смена пароля
- [ ] TwoFactorAuthentication.cshtml - настройка 2FA
- [ ] PersonalData.cshtml - персональные данные
- [ ] ExternalLogins.cshtml - внешние логины
- [ ] PhoneNumber.cshtml - управление телефоном

## 7. Configuration (Конфигурация)

### appsettings.json:

- [ ] Добавить секцию Twilio для SMS
- [ ] Добавить настройки 2FA
- [ ] Добавить настройки ролей

## 8. Testing (Тестирование)

### Что протестировать:

- [ ] Регистрация с дополнительными полями
- [ ] Редактирование профиля
- [ ] Подтверждение телефона через SMS
- [ ] 2FA через SMS и TOTP
- [ ] Роли и авторизация
- [ ] Смена пароля и email

## Приоритет выполнения:

1. Custom User Fields (базовая функциональность)
2. Roles (авторизация)
3. Phone Number Verification (SMS)
4. Two-Factor Authentication (безопасность)
5. Additional Features (расширенная функциональность)
