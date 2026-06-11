FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Orchestrator.Front -c Release -o /app/wwwroot

FROM caddy:2-alpine
COPY docker/caddy/Caddyfile /etc/caddy/Caddyfile
COPY --from=build /app/wwwroot/wwwroot /usr/share/caddy
EXPOSE 80 443
