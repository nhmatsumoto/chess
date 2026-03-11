FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY src/Chess.Domain/*.csproj src/Chess.Domain/
COPY src/Chess.Application/*.csproj src/Chess.Application/
COPY src/Chess.Infrastructure/*.csproj src/Chess.Infrastructure/
COPY src/Chess.WebApi/*.csproj src/Chess.WebApi/

RUN dotnet restore src/Chess.WebApi/Chess.WebApi.csproj

# Copy everything else and build
COPY src/ src/
RUN dotnet publish src/Chess.WebApi/Chess.WebApi.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "Chess.WebApi.dll"]
