# Используем .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем sln
COPY Web_Registration/Web_Registration.sln .

# Копируем проекты так, чтобы пути совпадали с .sln
COPY Web_Registration/Web_Registration.csproj .
COPY Write_to_DB3/Write_to_DB3/Write_to_DB3.csproj Write_to_DB3/Write_to_DB3/

# Восстанавливаем зависимости
RUN dotnet restore "Web_Registration.sln"

# Копируем всё остальное
COPY Web_Registration/. .
COPY Write_to_DB3/. Write_to_DB3/

# Сборка
RUN dotnet publish "Web_Registration.csproj" -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Web_Registration.dll"]
