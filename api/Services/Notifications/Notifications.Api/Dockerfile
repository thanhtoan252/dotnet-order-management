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
COPY Services/Notifications/Notifications.Domain/Notifications.Domain.csproj Services/Notifications/Notifications.Domain/
COPY Services/Notifications/Notifications.Application/Notifications.Application.csproj Services/Notifications/Notifications.Application/
COPY Services/Notifications/Notifications.Infrastructure/Notifications.Infrastructure.csproj Services/Notifications/Notifications.Infrastructure/
COPY Services/Notifications/Notifications.Api/Notifications.Api.csproj Services/Notifications/Notifications.Api/

RUN dotnet restore Services/Notifications/Notifications.Api/Notifications.Api.csproj

# Copy only the source directories needed by this image
COPY Shared/Shared.Core/ Shared/Shared.Core/
COPY Shared/Shared.Contracts/ Shared/Shared.Contracts/
COPY Shared/Shared.Messaging.Abstractions/ Shared/Shared.Messaging.Abstractions/
COPY Shared/Shared.Messaging/ Shared/Shared.Messaging/
COPY Shared/Shared.Observability/ Shared/Shared.Observability/
COPY Shared/Shared.Web/ Shared/Shared.Web/
COPY Services/Notifications/Notifications.Domain/ Services/Notifications/Notifications.Domain/
COPY Services/Notifications/Notifications.Application/ Services/Notifications/Notifications.Application/
COPY Services/Notifications/Notifications.Infrastructure/ Services/Notifications/Notifications.Infrastructure/
COPY Services/Notifications/Notifications.Api/ Services/Notifications/Notifications.Api/
WORKDIR /build/Services/Notifications/Notifications.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notifications.Api.dll"]
