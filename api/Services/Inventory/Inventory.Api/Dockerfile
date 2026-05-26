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
COPY Shared/Shared.Messaging.Abstractions/Shared.Messaging.Abstractions.csproj Shared/Shared.Messaging.Abstractions/
COPY Shared/Shared.Messaging/Shared.Messaging.csproj Shared/Shared.Messaging/
COPY Shared/Shared.Observability/Shared.Observability.csproj Shared/Shared.Observability/
COPY Shared/Shared.Web/Shared.Web.csproj Shared/Shared.Web/
COPY Services/Inventory/Inventory.Domain/Inventory.Domain.csproj Services/Inventory/Inventory.Domain/
COPY Services/Inventory/Inventory.Application/Inventory.Application.csproj Services/Inventory/Inventory.Application/
COPY Services/Inventory/Inventory.Infrastructure/Inventory.Infrastructure.csproj Services/Inventory/Inventory.Infrastructure/
COPY Services/Inventory/Inventory.Api/Inventory.Api.csproj Services/Inventory/Inventory.Api/

RUN dotnet restore Services/Inventory/Inventory.Api/Inventory.Api.csproj

# Copy only the source directories needed by this image
COPY Shared/Shared.Core/ Shared/Shared.Core/
COPY Shared/Shared.Contracts/ Shared/Shared.Contracts/
COPY Shared/Shared.Messaging.Abstractions/ Shared/Shared.Messaging.Abstractions/
COPY Shared/Shared.Messaging/ Shared/Shared.Messaging/
COPY Shared/Shared.Observability/ Shared/Shared.Observability/
COPY Shared/Shared.Web/ Shared/Shared.Web/
COPY Services/Inventory/Inventory.Domain/ Services/Inventory/Inventory.Domain/
COPY Services/Inventory/Inventory.Application/ Services/Inventory/Inventory.Application/
COPY Services/Inventory/Inventory.Infrastructure/ Services/Inventory/Inventory.Infrastructure/
COPY Services/Inventory/Inventory.Api/ Services/Inventory/Inventory.Api/
WORKDIR /build/Services/Inventory/Inventory.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
