# 1️⃣ Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy project file(s)
COPY *.csproj ./
RUN dotnet restore

# Copy rest of the code
COPY . ./

# Publish
RUN dotnet publish -c Release -o out

# 2️⃣ Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "EcommerceStore.dll"]
