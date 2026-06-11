FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Orchestrator.Api/Orchestrator.Api.csproj src/Orchestrator.Api/
COPY src/Orchestrator.Infrastructure/Orchestrator.Infrastructure.csproj src/Orchestrator.Infrastructure/
COPY src/Orchestrator.Application/Orchestrator.Application.csproj src/Orchestrator.Application/
COPY src/Orchestrator.Domain/Orchestrator.Domain.csproj src/Orchestrator.Domain/
COPY src/Orchestrator.Contracts/Orchestrator.Contracts.csproj src/Orchestrator.Contracts/
RUN dotnet restore src/Orchestrator.Api/Orchestrator.Api.csproj

COPY . .
RUN dotnet publish src/Orchestrator.Api/Orchestrator.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Orchestrator.Api.dll"]
