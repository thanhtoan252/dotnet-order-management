FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

# Copy central package management and solution
COPY Directory.Packages.props .
COPY OrderManagement.slnx .

# Copy project files for restore
COPY Shared/Shared.Core/Shared.Core.csproj Shared/Shared.Core/
COPY Shared/Shared.Contracts/Shared.Contracts.csproj Shared/Shared.Contracts/
COPY Shared/Shared.Messaging/Shared.Messaging.csproj Shared/Shared.Messaging/
COPY Services/Inventory/Inventory.Domain/Inventory.Domain.csproj Services/Inventory/Inventory.Domain/
COPY Services/Inventory/Inventory.Application/Inventory.Application.csproj Services/Inventory/Inventory.Application/
COPY Services/Inventory/Inventory.Infrastructure/Inventory.Infrastructure.csproj Services/Inventory/Inventory.Infrastructure/
COPY Services/Inventory/Inventory.MigrationRunner/Inventory.MigrationRunner.csproj Services/Inventory/Inventory.MigrationRunner/

RUN dotnet restore Services/Inventory/Inventory.MigrationRunner/Inventory.MigrationRunner.csproj

# Copy everything and build
COPY . .
WORKDIR /build/Services/Inventory/Inventory.MigrationRunner
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Inventory.MigrationRunner.dll"]