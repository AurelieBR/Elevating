# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files first so Docker can cache package restoration.
COPY ["src/Elevating.Domain/Elevating.Domain.csproj", "src/Elevating.Domain/"]
COPY ["src/Elevating.Application/Elevating.Application.csproj", "src/Elevating.Application/"]
COPY ["src/Elevating.Infrastructure/Elevating.Infrastructure.csproj", "src/Elevating.Infrastructure/"]
COPY ["src/Elevating.Api/Elevating.Api.csproj", "src/Elevating.Api/"]

RUN dotnet restore "src/Elevating.Api/Elevating.Api.csproj"

# Copy the remaining source code.
COPY . .

# Publish a Release build.
RUN dotnet publish "src/Elevating.Api/Elevating.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false


# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# ASP.NET Core container images use port 8080 by default.
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Elevating.Api.dll"]