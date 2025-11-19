# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Hospital_Patient_Intake_Scheduling_API_604.23E/Hospital_Patient_Intake_Scheduling_API_604.23E.csproj", "Hospital_Patient_Intake_Scheduling_API_604.23E/"]
RUN dotnet restore "./Hospital_Patient_Intake_Scheduling_API_604.23E/Hospital_Patient_Intake_Scheduling_API_604.23E.csproj"
COPY . .
WORKDIR "/src/Hospital_Patient_Intake_Scheduling_API_604.23E"
RUN dotnet build "./Hospital_Patient_Intake_Scheduling_API_604.23E.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Hospital_Patient_Intake_Scheduling_API_604.23E.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hospital_Patient_Intake_Scheduling_API_604.23E.dll", "--urls=http://0.0.0.0:8080"]