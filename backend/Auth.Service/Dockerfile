# ────────────────────────────────────────────────────────────────
# Stage 1 – Build
# ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy every project file first so layer-caching skips restore when source hasn't changed
COPY Auth.Service.Domain/Auth.Service.Domain.csproj             Auth.Service.Domain/
COPY Auth.Service.Application/Auth.Service.Application.csproj   Auth.Service.Application/
COPY Auth.Service.Infrastructure/Auth.Service.Infrastructure.csproj Auth.Service.Infrastructure/
COPY Auth.Service.Presentation/Auth.Service.Presentation.csproj Auth.Service.Presentation/

RUN dotnet restore Auth.Service.Presentation/Auth.Service.Presentation.csproj

# Copy all source files
COPY . .

RUN dotnet publish Auth.Service.Presentation/Auth.Service.Presentation.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# ────────────────────────────────────────────────────────────────
# Stage 2 – Runtime
# ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# ASP.NET Core listens on 8080 inside the container by default (.NET 8+)
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Auth.Service.Presentation.dll"]
