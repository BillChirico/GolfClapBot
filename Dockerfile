# Use the .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0.101 AS build-env
WORKDIR /app

# Copy csproj files and restore as distinct layers
COPY src/GolfClapBot.Runner/*.csproj ./src/GolfClapBot.Runner/
COPY src/GolfClapBot.Bot/*.csproj ./src/GolfClapBot.Bot/
COPY src/GolfClapBot.Domain/*.csproj ./src/GolfClapBot.Domain/
RUN dotnet restore src/GolfClapBot.Runner/GolfClapBot.Runner.csproj

# Copy everything else and build
COPY src/ ./src/
WORKDIR /app/src/GolfClapBot.Runner
RUN dotnet build -c Release -o out

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/src/GolfClapBot.Runner/out .
ENTRYPOINT ["dotnet", "GolfClapBot.Runner.dll"]
