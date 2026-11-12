# Техносклад

Тестовое задание: учёт техники на складе — справочники, позиции с фото, поиск, фильтры, realtime-обновления.

## Стек

- .NET 10, ASP.NET Core, EF Core + PostgreSQL, Blazor WebAssembly
- SignalR (тихое обновление списков), Scalar UI для OpenAPI
- Docker Compose для запуска

## Быстрый старт (Docker)

```bash
cd deploy
cp .env.example .env   # задать POSTGRES_PASSWORD
docker compose up --build
```

- Фронт: http://localhost:8080
- API / Scalar: http://localhost:5020/scalar

При старте API применяет миграции и заливает демо-данные (20 позиций с фото).

## Локальный запуск

Нужны .NET 10 SDK и PostgreSQL.

```bash
dotnet run --project src/TechStore.Api   # http://localhost:5020
dotnet run --project src/TechStore.Web   # http://localhost:5111
```

Строка подключения по умолчанию — `localhost:5432/techstore` (см. `src/TechStore.Api/appsettings.json`),
переопределяется через `ConnectionStrings__Default`. Фронт уже настроен на API (`ApiBaseUrl`).

## Структура

- `src/TechStore.Domain` — сущности, исключения
- `src/TechStore.Application` — сервисы, валидация (Result-паттерн)
- `src/TechStore.Infrastructure` — EF Core, файлы, SignalR, сидер
- `src/TechStore.Api` — контроллеры, Scalar, health-checks
- `src/TechStore.Web` — Blazor WASM на Bootstrap
- `tests/` — unit-тесты
- `deploy/` — compose, Dockerfile'ы, nginx
