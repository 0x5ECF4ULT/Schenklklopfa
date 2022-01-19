FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Schenklklopfa/Schenklklopfa.csproj", "Schenklklopfa/"]
RUN dotnet restore "Schenklklopfa/Schenklklopfa.csproj"
COPY . .
WORKDIR "/src/Schenklklopfa"
RUN dotnet build "Schenklklopfa.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Schenklklopfa.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Schenklklopfa.dll"]
