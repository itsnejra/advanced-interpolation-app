# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy csproj and restore
COPY ["InterpolationApp/InterpolationApp.csproj", "InterpolationApp/"]
RUN dotnet restore "InterpolationApp/InterpolationApp.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/InterpolationApp"
RUN dotnet build "InterpolationApp.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "InterpolationApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime (minimal Alpine image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
EXPOSE 8080

# Copy published files
COPY --from=publish /app/publish .

# Create non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

ENTRYPOINT ["dotnet", "InterpolationApp.dll"]
