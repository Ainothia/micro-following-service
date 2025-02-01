# Use official .NET 8 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application
COPY . ./
RUN dotnet publish -c Release -o /publish

# Use a smaller .NET 8 runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5100
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Expose API port for Swagger
EXPOSE 5100

# Run the application
ENTRYPOINT ["dotnet", "FollowService.dll"]
