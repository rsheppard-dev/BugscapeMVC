# Use the .NET 7 SDK as the base image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

# Set the working directory
WORKDIR /app

# Copy the .csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the project files
COPY . ./

# Build the project
RUN dotnet publish -c Release -o out

# Use the .NET 7 runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Set the working directory
WORKDIR /app

# Copy the build output from the build-env image
COPY --from=build-env /app/out .

# Specify the command to run on container startup
ENTRYPOINT ["dotnet", "Bugscape.dll"]