# syntax=docker/dockerfile:1

# --- 1. Base runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Системные зависимости для QuestPDF (кириллические шрифты + fontconfig) и healthcheck
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        fontconfig \
        fonts-dejavu-core \
        fonts-liberation \
        curl \
    && fc-cache -f -v \
    && rm -rf /var/lib/apt/lists/*

# Каталоги для пользовательских загрузок с правами для non-root пользователя ($APP_UID)
RUN mkdir -p \
        /app/wwwroot/images/books \
        /app/wwwroot/images/blogs \
        /app/wwwroot/images/chapters \
        /app/wwwroot/images/avatars \
        /home/app/.aspnet/DataProtection-Keys \
    && chown -R $APP_UID:$APP_UID /app /home/app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# --- 2. Build & publish stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем сначала файлы решения и проектов для кэширования слоя restore
COPY ["MajorAuthor.sln", "./"]
COPY ["MajorAuthor/MajorAuthor.csproj", "MajorAuthor/"]
COPY ["MajorAuthor.Data/MajorAuthor.Data.csproj", "MajorAuthor.Data/"]
RUN dotnet restore "./MajorAuthor/MajorAuthor.csproj"

# Копируем исходный код и выполняем publish за один проход (--no-restore)
COPY ["MajorAuthor/", "MajorAuthor/"]
COPY ["MajorAuthor.Data/", "MajorAuthor.Data/"]
RUN dotnet publish "./MajorAuthor/MajorAuthor.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# --- 3. Final runtime stage ---
FROM base AS final
WORKDIR /app
COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .

USER $APP_UID

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MajorAuthor.dll"]
