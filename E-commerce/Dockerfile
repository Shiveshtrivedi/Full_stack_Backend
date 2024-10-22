# Use the SDK image to build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj and restore dependencies
COPY ["E-commerce/E-commerce.csproj", "E-commerce/"]
RUN dotnet restore "E-commerce/E-commerce.csproj"

# Copy the remaining files and build the application
COPY . .
WORKDIR "/src/E-commerce"
RUN dotnet build "E-commerce.csproj" -c Release -o /app/build

# Publish the .NET application
FROM build AS publish
RUN dotnet publish "E-commerce.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image for .NET and install Mosquitto
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Install Mosquitto and Mosquitto clients
RUN apt-get update && apt-get install -y mosquitto mosquitto-clients



# Create necessary directories for Mosquitto
RUN mkdir -p /run/mosquitto && chown mosquitto:mosquitto /run/mosquitto


# Copy the Mosquitto configuration file if you have a custom one
COPY E-commerce/mosquitto.conf /etc/mosquitto/mosquitto.conf
# Copy the Mosquitto configuration file from the root directory
#COPY mosquitto.conf /mosquitto/config/mosquitto.conf
# Copy the published .NET application
WORKDIR /app
COPY --from=publish /app/publish .

# Expose the necessary ports for both .NET and Mosquitto
EXPOSE 80 1883 9001


# Start Mosquitto in the foreground and the .NET application


CMD mosquitto -c  /etc/mosquitto/mosquitto.conf & dotnet E-commerce.dll

