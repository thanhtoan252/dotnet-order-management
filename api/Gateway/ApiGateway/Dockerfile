FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

# Copy central package management
COPY Directory.Packages.props .
COPY OrderManagement.slnx .

# Copy project file for restore
COPY Gateway/ApiGateway/ApiGateway.csproj Gateway/ApiGateway/

RUN dotnet restore Gateway/ApiGateway/ApiGateway.csproj

# Copy everything and build
COPY . .
WORKDIR /build/Gateway/ApiGateway
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
