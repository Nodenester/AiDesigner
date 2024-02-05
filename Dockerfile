#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Install necessary tools, including Python
RUN apt-get update && \
    apt-get install -y python3 && \
    ln -s /usr/bin/python3 /usr/bin/python

# Install the wasm-tools workload
RUN dotnet workload install wasm-tools

COPY ["AiDesigner/Server/AiDesigner.Server.csproj", "AiDesigner/Server/"]
COPY ["AiDesigner/Client/AiDesigner.Client.csproj", "AiDesigner/Client/"]
COPY ["AiDesigner/Shared/AiDesigner.Shared.csproj", "AiDesigner/Shared/"]
RUN dotnet restore "./AiDesigner/Server/./AiDesigner.Server.csproj"
COPY . .
WORKDIR "/src/AiDesigner/Server"
RUN dotnet build "./AiDesigner.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AiDesigner.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AiDesigner.Server.dll"]