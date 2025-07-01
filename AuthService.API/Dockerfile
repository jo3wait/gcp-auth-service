# ───────── build ─────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AuthService.Domain/AuthService.Domain.csproj", "AuthService.Domain/"]
COPY ["AuthService.Application/AuthService.Application.csproj", "AuthService.Application/"]
COPY ["AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService.Infrastructure/"]
COPY ["AuthService.API/AuthService.API.csproj", "AuthService.API/"]
RUN dotnet restore "AuthService.API/AuthService.API.csproj"
COPY . .
RUN dotnet publish "AuthService.API/AuthService.API.csproj" -c Release -o /app/publish

# ───────── runtime ─────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS="https://+:8081;http://+:8080" \
    ASPNETCORE_HTTPS_PORT=8081 \
    ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080 8081
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
