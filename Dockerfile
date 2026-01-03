#
# Explicit container build for hosts where autodetection fails.
#
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + project files first for better layer caching.
COPY SmartStickyReviewer.sln ./
COPY src/SmartStickyReviewer.Api/SmartStickyReviewer.Api.csproj src/SmartStickyReviewer.Api/
COPY src/SmartStickyReviewer.Domain/SmartStickyReviewer.Domain.csproj src/SmartStickyReviewer.Domain/
COPY src/SmartStickyReviewer.Application/SmartStickyReviewer.Application.csproj src/SmartStickyReviewer.Application/
COPY src/SmartStickyReviewer.Infrastructure/SmartStickyReviewer.Infrastructure.csproj src/SmartStickyReviewer.Infrastructure/

RUN dotnet restore SmartStickyReviewer.sln

# Copy the rest of the source and publish the API.
COPY src/ src/
RUN dotnet publish src/SmartStickyReviewer.Api/SmartStickyReviewer.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .
# Bind to $PORT when provided by the host, otherwise 8080.
ENTRYPOINT ["sh", "-c", "exec dotnet SmartStickyReviewer.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
