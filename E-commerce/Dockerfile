# Use the official .NET SDK image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

# Set the working directory
WORKDIR /app

# Copy the .csproj file and restore any dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Build the application
RUN dotnet publish -c Release -o out

# Use the runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Set the working directory in the final image
WORKDIR /app

# Copy the build output from the build stage
COPY --from=build-env /app/out .

# Expose the necessary ports
# Change the ports according to your application's needs
EXPOSE 80
EXPOSE 9001

# Set the entry point for the application
ENTRYPOINT ["dotnet", "YourAppName.dll"]
