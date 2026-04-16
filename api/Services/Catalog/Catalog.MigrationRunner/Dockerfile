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
COPY Services/Catalog/Catalog.Domain/Catalog.Domain.csproj Services/Catalog/Catalog.Domain/
COPY Services/Catalog/Catalog.Application/Catalog.Application.csproj Services/Catalog/Catalog.Application/
COPY Services/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj Services/Catalog/Catalog.Infrastructure/
COPY Services/Catalog/Catalog.MigrationRunner/Catalog.MigrationRunner.csproj Services/Catalog/Catalog.MigrationRunner/

RUN dotnet restore Services/Catalog/Catalog.MigrationRunner/Catalog.MigrationRunner.csproj

# Copy everything and build
COPY . .
WORKDIR /build/Services/Catalog/Catalog.MigrationRunner
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Catalog.MigrationRunner.dll"]
