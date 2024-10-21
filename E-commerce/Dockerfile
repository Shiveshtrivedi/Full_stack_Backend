# Use the SDK image to build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY ["./E-commerce.csproj", "E-commerce/"]
RUN dotnet restore "./E-commerce.csproj"
COPY . .
WORKDIR "/src/E-commerce"
RUN dotnet build "E-commerce.csproj" -c Release -o /app/build

# Publish the .NET application
FROM build AS publish
RUN dotnet publish "E-commerce.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the base image for both .NET and Mosquitto
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Install Mosquitto
RUN apt-add-repository ppa:mosquitto-dev/mosquitto-ppa
RUN apt-get update
RUN apt-get install mosquitto
RUN apt-get install mosquitto-clients
RUN mosquitto

RUN pwd 
#RUN ls app/publish/
#RUN ls -la 
#RUN ls -la /E-commerce
#RUN ls -la /E-commerce/Configurations

RUN ls -la /app/publish/Configurations

# Copy the Mosquitto configuration file from the project directory
#COPY /app/publish/Configurations/mosquitto.conf /etc/mosquitto/mosquitto.conf
RUN mkdir -p /app/publish/Configurations/mosquitto.conf
RUN mkdir -p /Configurations/mosquitto.conf
COPY ./mosquitto.conf  /etc/mosquitto/mosquitto.conf


# Create the necessary directories for Mosquitto
RUN mkdir -p /run/mosquitto && chown mosquitto:mosquitto /run/mosquitto


# Copy the published .NET application
WORKDIR /app
COPY --from=publish /app/publish .

# Expose the necessary ports
EXPOSE 80 1883 9001

# Start Mosquitto in the foreground and the .NET application
CMD mosquitto & dotnet E-commerce.dll
#-c /etc/mosquitto/mosquitto.conf 
