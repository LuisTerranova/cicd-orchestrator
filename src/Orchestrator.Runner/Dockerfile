FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Orchestrator.Runner/Orchestrator.Runner.csproj src/Orchestrator.Runner/
COPY src/Orchestrator.Contracts/Orchestrator.Contracts.csproj src/Orchestrator.Contracts/
RUN dotnet restore src/Orchestrator.Runner/Orchestrator.Runner.csproj

COPY . .
RUN dotnet publish src/Orchestrator.Runner/Orchestrator.Runner.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Orchestrator.Runner.dll"]
