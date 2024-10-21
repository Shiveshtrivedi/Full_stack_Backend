# Use the SDK image to build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["E-commerce/E-commerce.csproj", "E-commerce/"]
RUN dotnet restore "E-commerce/E-commerce.csproj"
COPY . .
WORKDIR "/src/E-commerce"
RUN dotnet build "E-commerce.csproj" -c Release -o /app/build

# Publish the .NET application
FROM build AS publish
RUN dotnet publish "E-commerce.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the base image for both .NET and Mosquitto
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Install Mosquitto
RUN apt-get update && apt-get install -y mosquitto mosquitto-clients

# Copy the published .NET application
WORKDIR /app
COPY --from=publish /app/publish .

# Copy Mosquitto configuration if you have one
# COPY mosquitto.conf /etc/mosquitto/mosquitto.conf

# Expose the necessary ports
EXPOSE 80 1883 9001

# Start Mosquitto and the .NET application
CMD service mosquitto start && dotnet E-commerce.dll
