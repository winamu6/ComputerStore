
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/ComputerStore.Web/ComputerStore.Web.csproj", "src/ComputerStore.Web/"]
COPY ["src/ComputerStore.Infrastructure/ComputerStore.Infrastructure.csproj", "src/ComputerStore.Infrastructure/"]
COPY ["src/ComputerStore.Application/ComputerStore.Application.csproj", "src/ComputerStore.Application/"]
COPY ["src/ComputerStore.Domain/ComputerStore.Domain.csproj", "src/ComputerStore.Domain/"]
COPY ["src/ComputerStore.Shared/ComputerStore.Shared.csproj", "src/ComputerStore.Shared/"]

RUN dotnet restore "src/ComputerStore.Web/ComputerStore.Web.csproj"

COPY . .

WORKDIR "/src/src/ComputerStore.Web"
RUN dotnet build "ComputerStore.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ComputerStore.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ComputerStore.Web.dll"]