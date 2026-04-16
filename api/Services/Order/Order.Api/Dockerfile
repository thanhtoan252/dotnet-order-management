FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

# Copy central package management and solution
COPY Directory.Packages.props .
COPY OrderManagement.slnx .

# Copy project files for restore
COPY Shared/Shared.Core/Shared.Core.csproj Shared/Shared.Core/
COPY Shared/Shared.Contracts/Shared.Contracts.csproj Shared/Shared.Contracts/
COPY Shared/Shared.Messaging/Shared.Messaging.csproj Shared/Shared.Messaging/
COPY Services/Order/Order.Domain/Order.Domain.csproj Services/Order/Order.Domain/
COPY Services/Order/Order.Application/Order.Application.csproj Services/Order/Order.Application/
COPY Services/Order/Order.Infrastructure/Order.Infrastructure.csproj Services/Order/Order.Infrastructure/
COPY Services/Order/Order.Api/Order.Api.csproj Services/Order/Order.Api/

RUN dotnet restore Services/Order/Order.Api/Order.Api.csproj

# Copy everything and build
COPY . .
WORKDIR /build/Services/Order/Order.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Order.Api.dll"]
