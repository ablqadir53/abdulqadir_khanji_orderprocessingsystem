FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OrderProcessingSystem/OrderProcessingSystem.csproj", "OrderProcessingSystem/"]
COPY ["OrderProcessingSystem.Tests/OrderProcessingSystem.Tests.csproj", "OrderProcessingSystem.Tests/"]
RUN dotnet restore "OrderProcessingSystem/OrderProcessingSystem.csproj"
COPY . .
WORKDIR "/src/OrderProcessingSystem"
RUN dotnet build "OrderProcessingSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderProcessingSystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderProcessingSystem.dll"]
