# Cache stage: restore dependencies only
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

# Copy minimal files needed for restore
COPY Directory.Packages.props .
COPY OrderManagement.slnx .
COPY Shared/Shared.Core/Shared.Core.csproj Shared/Shared.Core/
COPY Shared/Shared.Observability/Shared.Observability.csproj Shared/Shared.Observability/
COPY Shared/Shared.Web/Shared.Web.csproj Shared/Shared.Web/
COPY Services/Identity/Identity.Application/Identity.Application.csproj Services/Identity/Identity.Application/
COPY Services/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj Services/Identity/Identity.Infrastructure/
COPY Services/Identity/Identity.Api/Identity.Api.csproj Services/Identity/Identity.Api/

RUN dotnet restore Services/Identity/Identity.Api/Identity.Api.csproj

# Build stage: reuses restored packages
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY --from=restore /src . /src/
COPY --from=restore /root/.nuget /root/.nuget

# Copy remaining source code
COPY . .

WORKDIR /src/Services/Identity/Identity.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage: minimal base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
USER app
COPY --from=build --chown=app:app /app/publish .
ENTRYPOINT ["dotnet", "Identity.Api.dll"]
