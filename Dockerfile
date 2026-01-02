# ==========================
# 1️⃣ Build Stage
# ==========================
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy solution file if exists
COPY *.sln ./

# Copy project files
COPY EcommerceStore/*.csproj ./EcommerceStore/
RUN dotnet restore

# Copy rest of the code
COPY . ./

# Publish app to /app/out
RUN dotnet publish "EcommerceStore/EcommerceStore.csproj" -c Release -o /app/out

# ==========================
# 2️⃣ Runtime Stage
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/out ./

# Expose default port
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "EcommerceStore.dll"]
