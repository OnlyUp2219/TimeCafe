# TimeCafe

---

<div align="center">
	<img src="https://img.shields.io/badge/TimeCafe-%F0%9F%8D%B5%20%F0%9F%8C%B1%20%F0%9F%91%A8%F0%9F%8F%BB%F0%9F%8F%92-blue" alt="TimeCafe" />
	<h2>TimeCafe — платформа для тайм-кафе</h2>
</div>

---

## 🚀 О проекте

TimeCafe — это современная платформа для управления тайм-кафе, включающая:

- Десктопное приложение (WinUI3)
- Веб-интерфейс (React + Vite)
- Микросервисы для авторизации, профиля, чата и основной логики

## 🏗️ Архитектура

Проект построен на основе микросервисов:

```text
TimeCafe.sln
├── DesktopApp/TimeCafe.UI         # Десктопное приложение
├── WebApp/timecafe.react.ui      # Веб-клиент
├── Services/                     # Микросервисы
│   ├── Auth                      # Авторизация и идентификация
│   ├── Chat                      # Модуль чата
│   ├── Main                      # Основная бизнес-логика
│   └── UserProfile               # Профиль пользователя
├── BuildingBlocks/               # Общие компоненты
```

---

## 🔐 Auth (Авторизация)

Модуль авторизации реализован на ASP.NET Identity с поддержкой:

- Кастомизации UI (страницы входа, регистрации, внешние провайдеры)
- Интеграции с Google/Microsoft
- Локализации интерфейса
- Расширения бизнес-логики (дополнительные поля, события)

**Пример кастомизации кнопок входа:**

```html
<!-- Google — красная -->
<button style="background-color: #dc3545; color: white;">Google</button>
<!-- Microsoft — зелёная -->
<button style="background-color: #28a745; color: white;">Microsoft</button>
```

**Скриншот структуры после scaffolding:**

```text
Areas/Identity/Pages/Account/
├── Login.cshtml
├── Register.cshtml
├── ExternalLogin.cshtml
├── RegisterConfirmation.cshtml
```

**Документация:** [Identity-Guide.md](Services/Auth/Identity-Guide.md)

---

## 👤 UserProfile (Профиль пользователя)

Модуль профиля реализует хранение и управление данными пользователя:

- Получение всех профилей и постраничная выборка
- Создание, обновление, удаление профиля
- Кэширование и оптимизация запросов

**Основные методы интерфейса:**

```csharp
public interface IUserRepositories
{
		Task<IEnumerable<Profile?>> GetAllProfilesAsync(...);
		Task<IEnumerable<Profile?>> GetProfilesPageAsync(int pageNumber, int pageSize, ...);
		Task<int> GetTotalPageAsync(...);
		Task<Profile?> GetProfileByIdAsync(string userId, ...);
		Task<Profile?> CreateProfileAsync(Profile profile, ...);
		Task<Profile?> UpdateProfileAsync(Profile profile, ...);
		Task DeleteProfileAsync(string userId, ...);
		Task CreateEmptyAsync(string userId, ...);
}
```

**Пример реализации (кэширование, логгирование):**

```csharp
public async Task<IEnumerable<Profile?>> GetAllProfilesAsync(...)
{
		var cached = await CacheHelper.GetAsync<IEnumerable<Profile>>(...);
		if (cached != null) return cached;
		var entity = await _context.Profiles.AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync(...);
		await CacheHelper.SetAsync(..., entity);
		return entity;
}
```

---

## 🛠️ Быстрый старт

### Предварительные требования

- .NET 9.0 SDK
- Node.js & npm
- JDK (для UML)
- Graphviz (для диаграмм)

### Сборка и запуск

<details>
<summary>Десктопное приложение</summary>

```powershell
cd DesktopApp/TimeCafe.UI
dotnet build
start TimeCafe.UI.exe
```

</details>

<details>
<summary>Веб-клиент</summary>

```powershell
cd WebApp/timecafe.react.ui
npm install
npm run dev
```

</details>

<details>
<summary>Микросервисы</summary>

```powershell
cd Services/Main/Main.TimeCafe.API
dotnet run
```

</details>

---

## 🧩 Используемые технологии

- **Backend**: ASP.NET Core, C#, Entity Framework
- **Frontend**: React, Vite, TailwindCSS
- **Desktop**: WinUI3
- **DevOps**: Docker, CI/CD, Graphviz, PlantUML

## 📚 Диаграммы и документация

- UML-диаграммы: `diagramClass-auth.md`, `sequence-auth.md`, `usecase-auth.puml`
- Документация по авторизации: `Services/Auth/Identity-Guide.md`

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

## Диаграммы и документация

- UML-диаграммы: `diagramClass-auth.md`, `sequence-auth.md`, `usecase-auth.puml`
- Документация по авторизации: `Services/Auth/Identity-Guide.md`

## Авторы и контакты

- OnlyUp2219 (архитектор, разработчик)
- [GitHub репозиторий](https://github.com/OnlyUp2219/TimeCafeWinUI3)

---

_Проект находится в активной разработке. Добро пожаловать к сотрудничеству!_
