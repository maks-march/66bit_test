# === Этап сборки ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Копируем файлы проектов API и его зависимостей, чтобы восстановить зависимости
# Копируем все .csproj, сохраняя структуру папок
COPY ["src/TechStore.Api/TechStore.Api.csproj", "src/TechStore.Api/"]
COPY ["src/TechStore.Application/TechStore.Application.csproj", "src/TechStore.Application/"]
COPY ["src/TechStore.Domain/TechStore.Domain.csproj", "src/TechStore.Domain/"]
COPY ["src/TechStore.Infrastructure/TechStore.Infrastructure.csproj", "src/TechStore.Infrastructure/"]

# 2. Восстанавливаем зависимости только для API (ProjectReference подтянут остальные проекты).
# Решение TechStore.slnx не используем: в контексте нет тестового проекта.
RUN dotnet restore "src/TechStore.Api/TechStore.Api.csproj"

# 3. Копируем весь исходный код и собираем проект API
COPY . .
WORKDIR "/src/src/TechStore.Api"
RUN dotnet publish "TechStore.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# === Этап выполнения ===
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Копируем опубликованные файлы из этапа сборки
COPY --from=build /app/publish .

# Запускаем приложение
ENTRYPOINT ["dotnet", "TechStore.Api.dll"]