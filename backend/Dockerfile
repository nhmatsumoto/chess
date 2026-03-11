# Use .NET 10 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy the entire src folder first to maintain structure correctly for restore
# This is safer than the loop for complex Clean Architecture setups
COPY src/ ./src/

# Restore the main project (which will restore all dependencies)
RUN dotnet restore src/Chess.WebApi/Chess.WebApi.csproj

# Build and Publish
RUN dotnet publish src/Chess.WebApi/Chess.WebApi.csproj -c Release -o /out --no-restore

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .

# Standard .NET 8+ port is 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "Chess.WebApi.dll"]
