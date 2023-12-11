# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/GolfClapBot.Runner/*.csproj ./src/GolfClapBot.Runner/
RUN dotnet restore src/GolfClapBot.Runner/GolfClapBot.Runner.csproj

# Copy everything else and build
COPY src/GolfClapBot.Runner/ ./src/GolfClapBot.Runner/
WORKDIR /app/src/GolfClapBot.Runner
RUN dotnet build -c Release -o out

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build-env /app/src/GolfClapBot.Runner/out .
ENTRYPOINT ["dotnet", "GolfClapBot.Runner.dll"]
