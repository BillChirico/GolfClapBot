FROM mcr.microsoft.com/dotnet/runtime:8.0-bookworm-slim AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GolfClapBot/GolfClapBot.Runner.csproj", "GolfClapBot/"]
RUN dotnet restore "GolfClapBot/GolfClapBot.Runner.csproj"
COPY . .
WORKDIR "/src/Volvox.Twitch.Responder"
RUN dotnet build "GolfClapBot.Runner.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GolfClapBot.Runner.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GolfClapBot.dll"]
