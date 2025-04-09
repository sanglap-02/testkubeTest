# Use the official .NET SDK image to build and run the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy everything
COPY . .

# Restore NuGet packages
RUN dotnet restore EndToEndAutomationTesting.sln

# Build the solution (Release mode by default)
RUN dotnet build EndToEndAutomationTesting.sln -c Release --no-restore

# Run tests (optional in build stage, or move to entrypoint)
#RUN dotnet test EndToEndAutomationTesting.sln -c Release --no-build --logger:trx

# Optional: Create a runtime image (if tests are a service or API)
# FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/out .

# Replace this with your test project's DLL name
ENTRYPOINT ["dotnet", "OutLineStruct.dll"]