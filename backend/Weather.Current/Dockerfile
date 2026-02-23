# ────────────────────────────────────────────────────────────────
# Stage 1 – Build
# ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy every project file first so layer-caching skips restore when source hasn't changed
COPY Weather.Current.Domain/Weather.Current.Domain.csproj                   Weather.Current.Domain/
COPY Weather.Current.Application/Weather.Current.Application.csproj         Weather.Current.Application/
COPY Weather.Current.Infrastructure/Weather.Current.Infrastructure.csproj   Weather.Current.Infrastructure/
COPY Weather.Current.Presentation/Weather.Current.Presentation.csproj       Weather.Current.Presentation/

RUN dotnet restore Weather.Current.Presentation/Weather.Current.Presentation.csproj

# Copy all source files
COPY . .

RUN dotnet publish Weather.Current.Presentation/Weather.Current.Presentation.csproj \
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

ENTRYPOINT ["dotnet", "Weather.Current.Presentation.dll"]
