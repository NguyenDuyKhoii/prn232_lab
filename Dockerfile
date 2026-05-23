# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["PRN232.LMS.API/PRN232.LMS.API.csproj", "PRN232.LMS.API/"]
COPY ["PRN232.LMS.Models/PRN232.LMS.Models.csproj", "PRN232.LMS.Models/"]
COPY ["PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj", "PRN232.LMS.Repositories/"]
COPY ["PRN232.LMS.Services/PRN232.LMS.Services.csproj", "PRN232.LMS.Services/"]

# Restore dependencies
RUN dotnet restore "PRN232.LMS.API/PRN232.LMS.API.csproj"

# Copy source code
COPY PRN232.LMS.API/ PRN232.LMS.API/
COPY PRN232.LMS.Models/ PRN232.LMS.Models/
COPY PRN232.LMS.Repositories/ PRN232.LMS.Repositories/
COPY PRN232.LMS.Services/ PRN232.LMS.Services/

# Build
WORKDIR /src/PRN232.LMS.API
RUN dotnet build "PRN232.LMS.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PRN232.LMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PRN232.LMS.API.dll", "--urls=http://0.0.0.0:8080"]