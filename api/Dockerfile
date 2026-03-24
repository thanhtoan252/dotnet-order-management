# syntax=docker/dockerfile:1
# ─── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy central package management file, solution, and all csproj files for layer caching
COPY Directory.Packages.props .
COPY *.slnx .
COPY --parents **/*.csproj ./

RUN dotnet restore OrderManagement.Api/OrderManagement.Api.csproj

# Copy source and publish
COPY . .
RUN dotnet publish OrderManagement.Api/OrderManagement.Api.csproj -c Release -o /app/publish

# ─── Stage 2: Runtime ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --chown=app:app --from=build /app/publish .

# Create logs directory owned by the built-in non-root user
RUN mkdir -p logs && chown -R app:app /app/logs

USER app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "OrderManagement.Api.dll"]
