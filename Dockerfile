# =============================================================================
# Smart Sticky Reviewer - Dockerfile for Tailway Deployment
# =============================================================================
# Multi-stage build for optimized .NET 8 deployment
# =============================================================================

# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (for better layer caching)
COPY SmartStickyReviewer.sln ./
COPY src/SmartStickyReviewer.Domain/SmartStickyReviewer.Domain.csproj ./src/SmartStickyReviewer.Domain/
COPY src/SmartStickyReviewer.Application/SmartStickyReviewer.Application.csproj ./src/SmartStickyReviewer.Application/
COPY src/SmartStickyReviewer.Infrastructure/SmartStickyReviewer.Infrastructure.csproj ./src/SmartStickyReviewer.Infrastructure/
COPY src/SmartStickyReviewer.Api/SmartStickyReviewer.Api.csproj ./src/SmartStickyReviewer.Api/

# Restore dependencies (cached if project files unchanged)
# Restore the deployable project (avoid requiring tests/ during container build)
RUN dotnet restore ./src/SmartStickyReviewer.Api/SmartStickyReviewer.Api.csproj

# Copy the rest of the source code
COPY src/ ./src/

# Build and publish in Release mode
RUN dotnet publish src/SmartStickyReviewer.Api/SmartStickyReviewer.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# Copy published application
COPY --from=build /app/publish .

# Copy widgets for static file serving
COPY widgets/ ./wwwroot/widgets/

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose the default port (will be overridden by PORT env var)
EXPOSE 8080

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:${PORT:-8080}/api/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "SmartStickyReviewer.Api.dll"]
