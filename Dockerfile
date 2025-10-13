# --- Этап сборки ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем всё в контейнер
COPY . .

# Восстанавливаем зависимости и собираем
RUN dotnet restore
RUN dotnet publish -c Release -o out

# --- Этап выполнения ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Указываем порт, который Railway будет слушать
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "project.dll"]
