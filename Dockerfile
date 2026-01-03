#
# Explicit container build for SmartStickyReviewer API (.NET 8)
# This avoids platform buildpack detection issues (e.g., Railpack).
#

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + project files first for better Docker layer caching
COPY SmartStickyReviewer.sln ./
COPY src/SmartStickyReviewer.Api/SmartStickyReviewer.Api.csproj src/SmartStickyReviewer.Api/
COPY src/SmartStickyReviewer.Domain/SmartStickyReviewer.Domain.csproj src/SmartStickyReviewer.Domain/
COPY src/SmartStickyReviewer.Application/SmartStickyReviewer.Application.csproj src/SmartStickyReviewer.Application/
COPY src/SmartStickyReviewer.Infrastructure/SmartStickyReviewer.Infrastructure.csproj src/SmartStickyReviewer.Infrastructure/
COPY tests/SmartStickyReviewer.Tests/SmartStickyReviewer.Tests.csproj tests/SmartStickyReviewer.Tests/

RUN dotnet restore "SmartStickyReviewer.sln"

# Copy the rest of the repo and publish the API
COPY . .
RUN dotnet publish "src/SmartStickyReviewer.Api/SmartStickyReviewer.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Most PaaS providers route traffic to a single HTTP port.
# If your platform provides PORT, the app will honor it (see Program.cs).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SmartStickyReviewer.Api.dll"]
