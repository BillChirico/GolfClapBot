FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Volvox.Twitch.Responder/Volvox.Twitch.Responder.csproj", "Volvox.Twitch.Responder/"]
RUN dotnet restore "Volvox.Twitch.Responder/Volvox.Twitch.Responder.csproj"
COPY . .
WORKDIR "/src/Volvox.Twitch.Responder"
RUN dotnet build "Volvox.Twitch.Responder.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Volvox.Twitch.Responder.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Volvox.Twitch.Responder.dll"]
