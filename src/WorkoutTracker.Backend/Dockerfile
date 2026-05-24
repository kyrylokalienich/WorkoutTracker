FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Restore (cached layer — only re-runs when .csproj files change)
COPY src/Domain/WorkoutTracker.Domain.csproj src/Domain/
COPY src/Application/WorkoutTracker.Application.csproj src/Application/
COPY src/Persistence/WorkoutTracker.Persistence.csproj src/Persistence/
COPY src/API/WorkoutTracker.API.csproj src/API/
RUN dotnet restore src/API/WorkoutTracker.API.csproj

COPY . .
RUN dotnet publish src/API/WorkoutTracker.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WorkoutTracker.API.dll"]
