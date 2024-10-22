
# Stage 1: Build the .NET Web API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project files
COPY E-commerce/*.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Build the application
RUN dotnet clean
RUN dotnet publish -c Release -o /app/out

# Stage 2: Create a runtime image with Mosquitto and the .NET application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install Mosquitto
RUN apt-get update && \
    apt-get install -y mosquitto mosquitto-clients && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Set the working directory for the .NET app
WORKDIR /app

# Copy the built application from the previous stage
COPY --from=build /app/out .

# Expose ports for both the Web API and Mosquitto
EXPOSE 80 1883 9001

# Copy Mosquitto configuration (optional)
COPY E-commerce/mosquitto.conf /etc/mosquitto/mosquitto.conf

# Start both Mosquitto and the .NET application
CMD ["sh", "-c", "mosquitto -c /etc/mosquitto/mosquitto.conf & dotnet EcommerceApi.dll"]