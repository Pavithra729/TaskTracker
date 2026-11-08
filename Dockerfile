# Stage 1: build with the full SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/TaskTracker.Api/TaskTracker.Api.csproj src/TaskTracker.Api/
RUN dotnet restore src/TaskTracker.Api/TaskTracker.Api.csproj

COPY src/TaskTracker.Api/ src/TaskTracker.Api/
RUN dotnet publish src/TaskTracker.Api/TaskTracker.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: run with the smaller ASP.NET runtime only
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN mkdir -p /app/data && chown $APP_UID /app/data
COPY --from=build /app/publish .

ENV ConnectionStrings__Default="Data Source=/app/data/tasks.db"
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "TaskTracker.Api.dll"]