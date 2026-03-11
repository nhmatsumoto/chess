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
# Install dependencies for Npgsql Kerberos support
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
COPY --from=build /out .

# Triple-redundancy for port 5000
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV ASPNETCORE_HTTP_PORTS=5000
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5000

ENTRYPOINT ["dotnet", "Chess.WebApi.dll"]
