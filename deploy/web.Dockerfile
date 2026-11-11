# === Этап сборки ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Копируем только файл проекта Web, чтобы восстановить его зависимости
COPY ["src/TechStore.Web/TechStore.Web.csproj", "src/TechStore.Web/"]
RUN dotnet restore "src/TechStore.Web/TechStore.Web.csproj"

# 2. Копируем весь код и публикуем в Release
COPY . .
WORKDIR "/src/src/TechStore.Web"
RUN dotnet publish "TechStore.Web.csproj" -c Release -o /app/publish

# === Этап выполнения (Nginx) ===
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Адрес API вшивается в appsettings.json статики (задаётся build-arg из compose)
ARG API_BASE_URL=http://localhost:8080

# Удаляем дефолтную страницу Nginx
RUN rm -rf ./*

# Копируем опубликованные статические файлы Blazor
COPY --from=build /app/publish/wwwroot .

# Подставляем адрес API для браузера (он ходит на хост, а не внутрь docker-сети)
RUN printf '{\n  "ApiBaseUrl": "%s"\n}\n' "$API_BASE_URL" > appsettings.json

# Копируем кастомный конфиг Nginx, если он нужен для SPA (fallback на index.html)
# Если конфига нет, Nginx будет отдавать 404 при обновлении страницы на внутреннем роуте.
# Раскомментируйте и создайте файл nginx.conf, если хотите исправить это.
# COPY deploy/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]